--- _Test.Breed.GetAll

SELECT *
FROM breed

--- _Test.UserSession.GetAll

SELECT *
FROM user_session

--- _Test.Quiz.GetAll

SELECT *
FROM quiz

--- _Test.QuizAnswer.GetAll

SELECT *
FROM quiz_answer

--- _Test.QuizResult.GetAll

SELECT *
FROM quiz_result

--- _Test.Breed.GetIds

SELECT id
FROM breed

--- _Test.Image.GetIds

SELECT id
FROM image
WHERE breed_id = @breedId
