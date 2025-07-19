CREATE TABLE users
(
    id             TEXT      NOT NULL PRIMARY KEY,
    name           TEXT      NOT NULL,
    email          TEXT      NOT NULL UNIQUE,
    email_verified BOOLEAN   NOT NULL DEFAULT FALSE,
    image          TEXT,
    created_at     TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at     TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    role           TEXT,
    banned         BOOLEAN   NOT NULL DEFAULT FALSE,
    ban_reason     TEXT,
    ban_expires    TIMESTAMP
);

CREATE TABLE sessions
(
    id                     TEXT      NOT NULL PRIMARY KEY,
    expires_at             TIMESTAMP NOT NULL,
    token                  TEXT      NOT NULL UNIQUE,
    created_at             TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at             TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ip_address             TEXT,
    user_agent             TEXT,
    user_id                TEXT      NOT NULL,
    impersonated_by        TEXT,
    active_organization_id TEXT,

    FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE
);

CREATE TABLE accounts
(
    id                      TEXT      NOT NULL PRIMARY KEY,
    account_id              TEXT      NOT NULL,
    provider_id             TEXT      NOT NULL,
    user_id                 TEXT      NOT NULL,
    access_token            TEXT,
    refresh_token           TEXT,
    id_token                TEXT,
    access_token_expires_at TIMESTAMP,
    scope                   TEXT,
    password                TEXT,
    created_at              TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at              TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,

    FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE
);

CREATE TABLE verifications
(
    id         TEXT      NOT NULL PRIMARY KEY,
    identifier TEXT      NOT NULL,
    value      TEXT      NOT NULL,
    expires_at TIMESTAMP NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE organizations
(
    id         TEXT      NOT NULL PRIMARY KEY,
    name       TEXT      NOT NULL,
    slug       TEXT      NOT NULL UNIQUE,
    logo       TEXT,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    metadata   TEXT
);

CREATE TABLE members
(
    id              TEXT      NOT NULL PRIMARY KEY,
    organization_id TEXT      NOT NULL,
    user_id         TEXT      NOT NULL,
    role            TEXT      NOT NULL,
    created_at      TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,

    FOREIGN KEY (organization_id) REFERENCES organizations (id) ON DELETE CASCADE,
    FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE
);

CREATE TABLE invitations
(
    id              TEXT      NOT NULL PRIMARY KEY,
    organization_id TEXT      NOT NULL,
    email           TEXT      NOT NULL,
    role            TEXT,
    status          TEXT      NOT NULL,
    expires_at      TIMESTAMP NOT NULL,
    inviter_id      TEXT      NOT NULL,

    FOREIGN KEY (organization_id) REFERENCES organizations (id) ON DELETE CASCADE,
    FOREIGN KEY (inviter_id) REFERENCES users (id) ON DELETE CASCADE
);

CREATE TABLE jwks
(
    id          TEXT      NOT NULL PRIMARY KEY,
    public_key  TEXT      NOT NULL,
    private_key TEXT      NOT NULL,
    created_at  TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);
