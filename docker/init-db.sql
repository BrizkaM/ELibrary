-- =============================================================================
-- PostgreSQL Initialization Script
-- =============================================================================
-- This script runs automatically when PostgreSQL container starts for the first time.
-- It creates the additional database needed for Keycloak.
-- =============================================================================

-- Create Keycloak database
CREATE DATABASE keycloak;

-- Grant permissions
GRANT ALL PRIVILEGES ON DATABASE keycloak TO elibrary;
