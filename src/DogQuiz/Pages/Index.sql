--- QuizResult.GetAll

SELECT quiz_id, score, timestamp
FROM quiz_result
WHERE user_session_id = @userSessionId
ORDER BY timestamp DESC

--- Quiz.Create

INSERT INTO quiz (id, expiration)
VALUES (@quizId, @expiration)

--- Image.GetRandom

SELECT image.filename
FROM image
ORDER BY RANDOM()
LIMIT 1

--- ImageBreed.GetRandom

WITH random_breed AS (
    SELECT id
    FROM breed
    GROUP BY breed.super_breed_id
    ORDER BY RANDOM()
)

SELECT image.id
FROM image
JOIN random_breed ON random_breed.id = image.breed_id
GROUP BY image.breed_id
ORDER BY RANDOM()
LIMIT @count

--- QuizQuestion.Create

INSERT INTO quiz_question (quiz_id, question_id, image_id)
VALUES (@quizId, @questionId, @imageId)

--- QuizCount.Bump

UPDATE quiz_count
SET val = val + 1
