--- Migration.0001_CreateDataLoadTables

CREATE TABLE super_breed (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    display_name TEXT NOT NULL UNIQUE
);

CREATE TABLE sub_breed (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    super_breed_id INTEGER NOT NULL REFERENCES super_breed(id),
    display_name TEXT NOT NULL,
    UNIQUE (super_breed_id, display_name)
);

CREATE TABLE breed (
    id INTEGER PRIMARY KEY,
    super_breed_id INTEGER NOT NULL REFERENCES super_breed(id),
    sub_breed_id INTEGER REFERENCES sub_breed(id),
    questions INTEGER NOT NULL,
    correct_answers INTEGER NOT NULL,
    UNIQUE (super_breed_id, sub_breed_id)
);

CREATE TABLE image (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    breed_id INTEGER NOT NULL REFERENCES breed(id),
    filename TEXT NOT NULL UNIQUE,
    file_version INTEGER NOT NULL,
    source_url TEXT NOT NULL
);
