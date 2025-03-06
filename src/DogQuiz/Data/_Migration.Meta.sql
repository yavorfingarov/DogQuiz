--- _Migration.EnsureTable

PRAGMA journal_mode = WAL;
PRAGMA synchronous = NORMAL;
PRAGMA busy_timeout = 5000;
PRAGMA cache_size = -20000;
PRAGMA foreign_keys = ON;
PRAGMA auto_vacuum = INCREMENTAL;
PRAGMA temp_store = MEMORY;
PRAGMA mmap_size = 2147483648;
PRAGMA page_size = 8192;

CREATE TABLE IF NOT EXISTS _migration (
    id TEXT PRIMARY KEY
);

--- _Migration.GetAll

SELECT id
FROM _migration

--- _Migration.Create

INSERT INTO _migration (id)
VALUES (@Id)
