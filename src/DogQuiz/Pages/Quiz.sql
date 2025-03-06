--- Quiz.Update

UPDATE quiz
SET expiration = @expiration
WHERE id = @quizId

--- QuizQuestion.GetImages

SELECT filename
FROM quiz_question_v
WHERE quiz_id = @quizId
ORDER BY question_id

--- QuizQuestion.GetBreedIds

SELECT breed_id
FROM quiz_question_v
WHERE quiz_id = @quizId
ORDER BY question_id

--- Breed.GetAll

SELECT id, super_breed, sub_breed
FROM breed_v

--- Breed.Update

UPDATE breed
SET questions = questions + 1,
    correct_answers = correct_answers + @correctAnswersIncrement
WHERE id = @breedId

--- QuizAnswer.Exists

SELECT 1
FROM quiz_answer
WHERE quiz_id = @quizId
  AND user_session_id = @userSessionId

--- QuizAnswer.Create

INSERT INTO quiz_answer (quiz_id, user_session_id, question_id, breed_id)
VALUES (@quizId, @userSessionId, @questionId, @breedId)

--- QuizResult.Create

INSERT INTO quiz_result (quiz_id, user_session_id, score, timestamp)
VALUES (@quizId, @userSessionId, @score, @timestamp)
