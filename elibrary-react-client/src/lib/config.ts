/**
 * Centralized application configuration.
 * 
 * All environment-specific values should be accessed through this module.
 * This provides:
 * - Type safety
 * - Validation on startup
 * - Single source of truth for configuration
 * 
 * Environment variables in Vite must be prefixed with VITE_ to be exposed.
 */

// =============================================================================
// Configuration Interface
// =============================================================================

interface AppConfig {
  /** API base URL without trailing slash */
  apiUrl: string;
  
  /** Keycloak configuration */
  keycloak: {
    /** Keycloak server URL without trailing slash */
    url: string;
    /** Realm name */
    realm: string;
    /** Client ID for this application */
    clientId: string;
    /** OAuth scopes to request */
    scopes: string;
  };
  
  /** Application settings */
  app: {
    /** Application display name */
    name: string;
    /** Default language code */
    defaultLanguage: 'cs' | 'en';
    /** Current environment */
    environment: 'development' | 'production' | 'staging';
  };
  
  /** Feature flags */
  features: {
    /** Use mock API instead of real backend */
    mockApi: boolean;
    /** Enable debug mode (extra logging, etc.) */
    debugMode: boolean;
  };
}

// =============================================================================
// Environment Variable Helpers
// =============================================================================

/**
 * Gets an environment variable value.
 * Throws an error if the variable is required but not set.
 */
function getEnvVar(key: string, required: boolean = true): string {
  const value = import.meta.env[key];
  
  if (required && (!value || value === '')) {
    throw new Error(
      `Missing required environment variable: ${key}. ` +
      `Please check your .env file.`
    );
  }
  
  return value || '';
}

/**
 * Gets a boolean environment variable.
 */
function getEnvBool(key: string, defaultValue: boolean = false): boolean {
  const value = import.meta.env[key];
  if (value === undefined || value === '') return defaultValue;
  return value === 'true' || value === '1';
}

// =============================================================================
// Configuration Object
// =============================================================================

/**
 * Application configuration loaded from environment variables.
 * Validated on module load - will throw if required values are missing.
 */
export const config: AppConfig = {
  apiUrl: getEnvVar('VITE_API_URL'),
  
  keycloak: {
    url: getEnvVar('VITE_KEYCLOAK_URL'),
    realm: getEnvVar('VITE_KEYCLOAK_REALM'),
    clientId: getEnvVar('VITE_KEYCLOAK_CLIENT_ID'),
    scopes: getEnvVar('VITE_KEYCLOAK_SCOPES', false) || 'openid profile email',
  },
  
  app: {
    name: getEnvVar('VITE_APP_NAME', false) || 'E-Library',
    defaultLanguage: (getEnvVar('VITE_DEFAULT_LANGUAGE', false) || 'cs') as 'cs' | 'en',
    environment: import.meta.env.MODE as 'development' | 'production' | 'staging',
  },
  
  features: {
    mockApi: getEnvBool('VITE_ENABLE_MOCK_API', false),
    debugMode: getEnvBool('VITE_ENABLE_DEBUG_MODE', import.meta.env.DEV),
  },
};

// =============================================================================
// Derived Configuration
// =============================================================================

/**
 * Full Keycloak authorization endpoint URL.
 */
export const keycloakAuthUrl = 
  `${config.keycloak.url}/realms/${config.keycloak.realm}/protocol/openid-connect/auth`;

/**
 * Full Keycloak token endpoint URL.
 */
export const keycloakTokenUrl = 
  `${config.keycloak.url}/realms/${config.keycloak.realm}/protocol/openid-connect/token`;

/**
 * Full Keycloak logout endpoint URL.
 */
export const keycloakLogoutUrl = 
  `${config.keycloak.url}/realms/${config.keycloak.realm}/protocol/openid-connect/logout`;

/**
 * Full Keycloak userinfo endpoint URL.
 */
export const keycloakUserInfoUrl = 
  `${config.keycloak.url}/realms/${config.keycloak.realm}/protocol/openid-connect/userinfo`;

/**
 * Keycloak JWKS (public keys) endpoint URL.
 */
export const keycloakJwksUrl = 
  `${config.keycloak.url}/realms/${config.keycloak.realm}/protocol/openid-connect/certs`;

// =============================================================================
// Debug Logging
// =============================================================================

if (config.features.debugMode) {
  console.log('🔧 Application Configuration:', {
    apiUrl: config.apiUrl,
    keycloak: {
      url: config.keycloak.url,
      realm: config.keycloak.realm,
      clientId: config.keycloak.clientId,
    },
    environment: config.app.environment,
    features: config.features,
  });
}

// =============================================================================
// Validation
// =============================================================================

/**
 * Validates that all required configuration is present.
 * Called automatically when this module is imported.
 */
function validateConfig(): void {
  const errors: string[] = [];
  
  if (!config.apiUrl.startsWith('http')) {
    errors.push('VITE_API_URL must be a valid URL starting with http:// or https://');
  }
  
  if (!config.keycloak.url.startsWith('http')) {
    errors.push('VITE_KEYCLOAK_URL must be a valid URL starting with http:// or https://');
  }
  
  if (!config.keycloak.realm) {
    errors.push('VITE_KEYCLOAK_REALM is required');
  }
  
  if (!config.keycloak.clientId) {
    errors.push('VITE_KEYCLOAK_CLIENT_ID is required');
  }
  
  if (errors.length > 0) {
    throw new Error(
      'Configuration validation failed:\n' +
      errors.map(e => `  - ${e}`).join('\n')
    );
  }
}

// Validate on load
validateConfig();

export default config;
