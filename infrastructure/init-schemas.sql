-- DC Platform - PostgreSQL Schema Initialization
-- Creates isolated schemas for each service (shared database, schema-per-service)

CREATE SCHEMA IF NOT EXISTS directory;
CREATE SCHEMA IF NOT EXISTS access_control;
CREATE SCHEMA IF NOT EXISTS audit;
CREATE SCHEMA IF NOT EXISTS configuration;
CREATE SCHEMA IF NOT EXISTS keycloak;
