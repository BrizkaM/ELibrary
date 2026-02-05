/**
 * Keycloak Authentication Service
 * 
 * Implements OAuth 2.0 Authorization Code Flow with PKCE for secure
 * authentication in Single Page Applications.
 * 
 * Flow:
 * 1. Generate PKCE code verifier and challenge
 * 2. Redirect to Keycloak login
 * 3. Handle callback with authorization code
 * 4. Exchange code for tokens
 * 5. Manage token refresh
 */

import { config } from "../lib/config";

// =============================================================================
// Types
// =============================================================================

export interface TokenResponse {
  access_token: string;
  refresh_token: string;
  id_token: string;
  expires_in: number;
  refresh_expires_in: number;
  token_type: string;
  scope: string;
}

export interface UserInfo {
  sub: string;
  email?: string;
  email_verified?: boolean;
  preferred_username?: string;
  given_name?: string;
  family_name?: string;
  name?: string;
  roles?: string[];
}

export interface DecodedToken {
  exp: number;
  iat: number;
  sub: string;
  preferred_username?: string;
  email?: string;
  given_name?: string;
  family_name?: string;
  realm_access?: {
    roles: string[];
  };
  resource_access?: {
    [clientId: string]: {
      roles: string[];
    };
  };
}

// =============================================================================
// PKCE Helpers
// =============================================================================

/**
 * Generates a cryptographically random string for PKCE code verifier
 */
function generateCodeVerifier(): string {
  const array = new Uint8Array(32);
  crypto.getRandomValues(array);
  return base64UrlEncode(array);
}

/**
 * Creates SHA-256 hash of code verifier for PKCE challenge
 */
async function generateCodeChallenge(verifier: string): Promise<string> {
  const encoder = new TextEncoder();
  const data = encoder.encode(verifier);
  const digest = await crypto.subtle.digest("SHA-256", data);
  return base64UrlEncode(new Uint8Array(digest));
}

/**
 * Base64 URL encoding (RFC 4648)
 */
function base64UrlEncode(buffer: Uint8Array): string {
  const base64 = btoa(String.fromCharCode(...buffer));
  return base64
    .replace(/\+/g, "-")
    .replace(/\//g, "_")
    .replace(/=+$/, "");
}

/**
 * Generates a random state parameter for CSRF protection
 */
function generateState(): string {
  const array = new Uint8Array(16);
  crypto.getRandomValues(array);
  return base64UrlEncode(array);
}

// =============================================================================
// Token Storage Keys
// =============================================================================

const STORAGE_KEYS = {
  CODE_VERIFIER: "elibrary_pkce_verifier",
  STATE: "elibrary_auth_state",
  ACCESS_TOKEN: "elibrary_access_token",
  REFRESH_TOKEN: "elibrary_refresh_token",
  ID_TOKEN: "elibrary_id_token",
  TOKEN_EXPIRY: "elibrary_token_expiry",
} as const;

// =============================================================================
// Keycloak Service
// =============================================================================

class KeycloakService {
  private readonly authUrl: string;
  private readonly tokenUrl: string;
  private readonly logoutUrl: string;
  private readonly userInfoUrl: string;
  private readonly clientId: string;
  private readonly redirectUri: string;
  private readonly scopes: string;

  constructor() {
    const keycloakUrl = config.keycloak.url;
    const realm = config.keycloak.realm;

    this.authUrl = `${keycloakUrl}/realms/${realm}/protocol/openid-connect/auth`;
    this.tokenUrl = `${keycloakUrl}/realms/${realm}/protocol/openid-connect/token`;
    this.logoutUrl = `${keycloakUrl}/realms/${realm}/protocol/openid-connect/logout`;
    this.userInfoUrl = `${keycloakUrl}/realms/${realm}/protocol/openid-connect/userinfo`;
    this.clientId = config.keycloak.clientId;
    this.redirectUri = `${window.location.origin}/auth/callback`;
    this.scopes = config.keycloak.scopes;

    if (config.features.debugMode) {
      console.log("🔐 KeycloakService initialized", {
        authUrl: this.authUrl,
        tokenUrl: this.tokenUrl,
        clientId: this.clientId,
        redirectUri: this.redirectUri,
      });
    }
  }

  // ===========================================================================
  // Login Flow
  // ===========================================================================

  /**
   * Initiates the login flow by redirecting to Keycloak
   */
  async login(): Promise<void> {
    // Generate PKCE values
    const codeVerifier = generateCodeVerifier();
    const codeChallenge = await generateCodeChallenge(codeVerifier);
    const state = generateState();

    // Store for callback verification
    sessionStorage.setItem(STORAGE_KEYS.CODE_VERIFIER, codeVerifier);
    sessionStorage.setItem(STORAGE_KEYS.STATE, state);

    // Build authorization URL
    const params = new URLSearchParams({
      client_id: this.clientId,
      redirect_uri: this.redirectUri,
      response_type: "code",
      scope: this.scopes,
      state: state,
      code_challenge: codeChallenge,
      code_challenge_method: "S256",
    });

    const authorizationUrl = `${this.authUrl}?${params.toString()}`;

    if (config.features.debugMode) {
      console.log("🔐 Redirecting to Keycloak login", { authorizationUrl });
    }

    // Redirect to Keycloak
    window.location.href = authorizationUrl;
  }

  // ===========================================================================
  // Callback Handling
  // ===========================================================================

  /**
   * Handles the callback from Keycloak after login
   */
  async handleCallback(searchParams: URLSearchParams): Promise<TokenResponse> {
    const code = searchParams.get("code");
    const state = searchParams.get("state");
    const error = searchParams.get("error");
    const errorDescription = searchParams.get("error_description");

    // Check for errors
    if (error) {
      throw new Error(`Authentication error: ${error} - ${errorDescription}`);
    }

    if (!code) {
      throw new Error("No authorization code received");
    }

    // Verify state (CSRF protection)
    const storedState = sessionStorage.getItem(STORAGE_KEYS.STATE);
    if (state !== storedState) {
      throw new Error("Invalid state parameter - possible CSRF attack");
    }

    // Get code verifier for PKCE
    const codeVerifier = sessionStorage.getItem(STORAGE_KEYS.CODE_VERIFIER);
    if (!codeVerifier) {
      throw new Error("No code verifier found - PKCE flow broken");
    }

    // Exchange code for tokens
    const tokens = await this.exchangeCodeForTokens(code, codeVerifier);

    // Clean up temporary storage
    sessionStorage.removeItem(STORAGE_KEYS.CODE_VERIFIER);
    sessionStorage.removeItem(STORAGE_KEYS.STATE);

    // Store tokens
    this.storeTokens(tokens);

    return tokens;
  }

  /**
   * Exchanges authorization code for tokens
   */
  private async exchangeCodeForTokens(
    code: string,
    codeVerifier: string
  ): Promise<TokenResponse> {
    const params = new URLSearchParams({
      grant_type: "authorization_code",
      client_id: this.clientId,
      redirect_uri: this.redirectUri,
      code: code,
      code_verifier: codeVerifier,
    });

    if (config.features.debugMode) {
      console.log("🔐 Exchanging code for tokens");
    }

    const response = await fetch(this.tokenUrl, {
      method: "POST",
      headers: {
        "Content-Type": "application/x-www-form-urlencoded",
      },
      body: params.toString(),
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(`Token exchange failed: ${error.error_description || error.error}`);
    }

    const tokens: TokenResponse = await response.json();

    if (config.features.debugMode) {
      console.log("🔐 Tokens received", {
        expiresIn: tokens.expires_in,
        scope: tokens.scope,
      });
    }

    return tokens;
  }

  // ===========================================================================
  // Token Refresh
  // ===========================================================================

  /**
   * Refreshes the access token using the refresh token
   */
  async refreshToken(): Promise<TokenResponse | null> {
    const refreshToken = localStorage.getItem(STORAGE_KEYS.REFRESH_TOKEN);

    if (!refreshToken) {
      if (config.features.debugMode) {
        console.log("🔐 No refresh token available");
      }
      return null;
    }

    try {
      const params = new URLSearchParams({
        grant_type: "refresh_token",
        client_id: this.clientId,
        refresh_token: refreshToken,
      });

      if (config.features.debugMode) {
        console.log("🔐 Refreshing access token");
      }

      const response = await fetch(this.tokenUrl, {
        method: "POST",
        headers: {
          "Content-Type": "application/x-www-form-urlencoded",
        },
        body: params.toString(),
      });

      if (!response.ok) {
        // Refresh token expired or invalid
        if (config.features.debugMode) {
          console.log("🔐 Token refresh failed, clearing tokens");
        }
        this.clearTokens();
        return null;
      }

      const tokens: TokenResponse = await response.json();
      this.storeTokens(tokens);

      if (config.features.debugMode) {
        console.log("🔐 Tokens refreshed successfully");
      }

      return tokens;
    } catch (error) {
      if (config.features.debugMode) {
        console.error("🔐 Token refresh error", error);
      }
      this.clearTokens();
      return null;
    }
  }

  // ===========================================================================
  // Logout
  // ===========================================================================

  /**
   * Logs out the user from Keycloak
   */
  logout(): void {
    const idToken = localStorage.getItem(STORAGE_KEYS.ID_TOKEN);

    // Clear local tokens first
    this.clearTokens();

    // Build logout URL
    const params = new URLSearchParams({
      client_id: this.clientId,
      post_logout_redirect_uri: window.location.origin,
    });

    // Include id_token_hint if available for better UX
    if (idToken) {
      params.append("id_token_hint", idToken);
    }

    const logoutUrl = `${this.logoutUrl}?${params.toString()}`;

    if (config.features.debugMode) {
      console.log("🔐 Logging out", { logoutUrl });
    }

    // Redirect to Keycloak logout
    window.location.href = logoutUrl;
  }

  // ===========================================================================
  // Token Management
  // ===========================================================================

  /**
   * Stores tokens in localStorage
   */
  private storeTokens(tokens: TokenResponse): void {
    localStorage.setItem(STORAGE_KEYS.ACCESS_TOKEN, tokens.access_token);
    localStorage.setItem(STORAGE_KEYS.REFRESH_TOKEN, tokens.refresh_token);
    localStorage.setItem(STORAGE_KEYS.ID_TOKEN, tokens.id_token);
    
    // Calculate and store expiry time
    const expiryTime = Date.now() + tokens.expires_in * 1000;
    localStorage.setItem(STORAGE_KEYS.TOKEN_EXPIRY, expiryTime.toString());
  }

  /**
   * Clears all stored tokens
   */
  clearTokens(): void {
    localStorage.removeItem(STORAGE_KEYS.ACCESS_TOKEN);
    localStorage.removeItem(STORAGE_KEYS.REFRESH_TOKEN);
    localStorage.removeItem(STORAGE_KEYS.ID_TOKEN);
    localStorage.removeItem(STORAGE_KEYS.TOKEN_EXPIRY);
  }

  /**
   * Gets the current access token
   */
  getAccessToken(): string | null {
    return localStorage.getItem(STORAGE_KEYS.ACCESS_TOKEN);
  }

  /**
   * Checks if the access token is expired
   */
  isTokenExpired(): boolean {
    const expiry = localStorage.getItem(STORAGE_KEYS.TOKEN_EXPIRY);
    if (!expiry) return true;

    // Add 30 second buffer for network latency
    return Date.now() > parseInt(expiry) - 30000;
  }

  /**
   * Checks if user is authenticated (has valid tokens)
   */
  isAuthenticated(): boolean {
    const token = this.getAccessToken();
    return token !== null && !this.isTokenExpired();
  }

  // ===========================================================================
  // User Info
  // ===========================================================================

  /**
   * Fetches user info from Keycloak
   */
  async getUserInfo(): Promise<UserInfo | null> {
    const accessToken = this.getAccessToken();
    if (!accessToken) return null;

    try {
      const response = await fetch(this.userInfoUrl, {
        headers: {
          Authorization: `Bearer ${accessToken}`,
        },
      });

      if (!response.ok) {
        return null;
      }

      return await response.json();
    } catch (error) {
      if (config.features.debugMode) {
        console.error("🔐 Failed to fetch user info", error);
      }
      return null;
    }
  }

  /**
   * Decodes the access token to get user info (without validation)
   * Note: This is for display purposes only. Backend validates the token.
   */
  decodeToken(): DecodedToken | null {
    const token = this.getAccessToken();
    if (!token) return null;

    try {
      const parts = token.split(".");
      if (parts.length !== 3) return null;

      const payload = parts[1];
      const decoded = atob(payload.replace(/-/g, "+").replace(/_/g, "/"));
      return JSON.parse(decoded);
    } catch {
      return null;
    }
  }

  /**
   * Gets user roles from the token
   */
  getUserRoles(): string[] {
    const decoded = this.decodeToken();
    if (!decoded) return [];

    const roles: string[] = [];

    // Realm roles
    if (decoded.realm_access?.roles) {
      roles.push(...decoded.realm_access.roles);
    }

    // Client roles
    if (decoded.resource_access?.[this.clientId]?.roles) {
      roles.push(...decoded.resource_access[this.clientId].roles);
    }

    return roles;
  }

  /**
   * Checks if user has a specific role
   */
  hasRole(role: string): boolean {
    return this.getUserRoles().includes(role);
  }
}

// Export singleton instance
export const keycloakService = new KeycloakService();
