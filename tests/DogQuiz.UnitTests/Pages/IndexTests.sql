--- _Test.Image.Exists

SELECT 1
FROM image
WHERE filename = @filename

--- _Test.QuizQuestionImage.GetAll

SELECT *
FROM quiz_question_v

--- _Test.QuizCount.GetAll

SELECT *
FROM quiz_count
