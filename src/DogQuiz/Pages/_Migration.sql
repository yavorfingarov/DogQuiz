--- Migration.0002_CreateQuizTables

CREATE TABLE user_session (
    id TEXT NOT NULL PRIMARY KEY,
    expiration TEXT NOT NULL
);

CREATE TABLE quiz (
    id TEXT NOT NULL PRIMARY KEY,
    expiration TEXT NOT NULL
);

CREATE TABLE quiz_question (
    quiz_id TEXT NOT NULL REFERENCES quiz(id) ON DELETE CASCADE,
    question_id INTEGER NOT NULL,
    image_id INTEGER NOT NULL REFERENCES image(id),
    PRIMARY KEY (quiz_id, question_id)
    UNIQUE (quiz_id, image_id)
);

CREATE TABLE quiz_answer (
    quiz_id TEXT NOT NULL REFERENCES quiz(id) ON DELETE CASCADE,
    user_session_id TEXT NOT NULL REFERENCES user_session(id) ON DELETE CASCADE,
    question_id INTEGER NOT NULL,
    breed_id INTEGER NOT NULL REFERENCES breed(id),
    PRIMARY KEY (quiz_id, user_session_id, question_id)
);

CREATE TABLE quiz_result (
    user_session_id TEXT REFERENCES user_session(id) ON DELETE SET NULL,
    quiz_id TEXT REFERENCES quiz(id) ON DELETE SET NULL,
    score REAL NOT NULL,
    timestamp TEXT NOT NULL,
    PRIMARY KEY (user_session_id, quiz_id)
);

CREATE TABLE quiz_count (
    val INTEGER NOT NULL
);

INSERT INTO quiz_count (val)
VALUES (0);

--- Migration.0003_CreateViews

CREATE VIEW breed_v AS
SELECT 
    breed.id,
    super_breed.display_name AS super_breed,
    sub_breed.display_name AS sub_breed,
    breed.questions,
    breed.correct_answers
FROM breed
JOIN super_breed ON super_breed.id = breed.super_breed_id
LEFT JOIN sub_breed ON sub_breed.id = breed.sub_breed_id;

CREATE VIEW quiz_question_v AS
SELECT quiz_question.quiz_id, quiz_question.question_id, image.breed_id, image.filename
FROM quiz_question
JOIN image ON image.id = quiz_question.image_id;
