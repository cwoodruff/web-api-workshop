-- Enable FK enforcement in SQLite
PRAGMA foreign_keys = ON;

BEGIN TRANSACTION;

-- AspNetRoles
CREATE TABLE IF NOT EXISTS AspNetRoles (
    Id               TEXT NOT NULL PRIMARY KEY,
    Name             TEXT NULL,
    NormalizedName   TEXT NULL,
    ConcurrencyStamp TEXT NULL
);

-- AspNetUsers
CREATE TABLE IF NOT EXISTS AspNetUsers (
    Id                   TEXT NOT NULL PRIMARY KEY,
    UserName             TEXT NULL,
    NormalizedUserName   TEXT NULL,
    Email                TEXT NULL,
    NormalizedEmail      TEXT NULL,
    EmailConfirmed       INTEGER NOT NULL,
    PasswordHash         TEXT NULL,
    SecurityStamp        TEXT NULL,
    ConcurrencyStamp     TEXT NULL,
    PhoneNumber          TEXT NULL,
    PhoneNumberConfirmed INTEGER NOT NULL,
    TwoFactorEnabled     INTEGER NOT NULL,
    LockoutEnd           TEXT NULL, -- store as ISO-8601 string
    LockoutEnabled       INTEGER NOT NULL,
    AccessFailedCount    INTEGER NOT NULL
);

-- AspNetRoleClaims
CREATE TABLE IF NOT EXISTS AspNetRoleClaims (
    Id         INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    RoleId     TEXT NOT NULL,
    ClaimType  TEXT NULL,
    ClaimValue TEXT NULL,
    CONSTRAINT FK_AspNetRoleClaims_AspNetRoles_RoleId
        FOREIGN KEY (RoleId) REFERENCES AspNetRoles (Id) ON DELETE CASCADE
);

-- AspNetUserClaims
CREATE TABLE IF NOT EXISTS AspNetUserClaims (
    Id         INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    UserId     TEXT NOT NULL,
    ClaimType  TEXT NULL,
    ClaimValue TEXT NULL,
    CONSTRAINT FK_AspNetUserClaims_AspNetUsers_UserId
        FOREIGN KEY (UserId) REFERENCES AspNetUsers (Id) ON DELETE CASCADE
);

-- AspNetUserLogins
CREATE TABLE IF NOT EXISTS AspNetUserLogins (
    LoginProvider      TEXT NOT NULL,
    ProviderKey        TEXT NOT NULL,
    ProviderDisplayName TEXT NULL,
    UserId             TEXT NOT NULL,
    PRIMARY KEY (LoginProvider, ProviderKey),
    CONSTRAINT FK_AspNetUserLogins_AspNetUsers_UserId
        FOREIGN KEY (UserId) REFERENCES AspNetUsers (Id) ON DELETE CASCADE
);

-- AspNetUserRoles
CREATE TABLE IF NOT EXISTS AspNetUserRoles (
    UserId TEXT NOT NULL,
    RoleId TEXT NOT NULL,
    PRIMARY KEY (UserId, RoleId),
    CONSTRAINT FK_AspNetUserRoles_AspNetRoles_RoleId
        FOREIGN KEY (RoleId) REFERENCES AspNetRoles (Id) ON DELETE CASCADE,
    CONSTRAINT FK_AspNetUserRoles_AspNetUsers_UserId
        FOREIGN KEY (UserId) REFERENCES AspNetUsers (Id) ON DELETE CASCADE
);

-- AspNetUserTokens
CREATE TABLE IF NOT EXISTS AspNetUserTokens (
    UserId        TEXT NOT NULL,
    LoginProvider TEXT NOT NULL,
    Name          TEXT NOT NULL,
    Value         TEXT NULL,
    PRIMARY KEY (UserId, LoginProvider, Name),
    CONSTRAINT FK_AspNetUserTokens_AspNetUsers_UserId
        FOREIGN KEY (UserId) REFERENCES AspNetUsers (Id) ON DELETE CASCADE
);

COMMIT;