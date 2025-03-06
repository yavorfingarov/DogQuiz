--- QuizResult.GetScore

SELECT score
FROM quiz_result
WHERE quiz_id = @quizId
  AND user_session_id = @userSessionId

--- QuizAnswer.GetDiff

SELECT 
    TRIM(COALESCE(answer_breed.sub_breed, '') || ' ' || answer_breed.super_breed) AS given,
    TRIM(COALESCE(correct_breed.sub_breed, '') || ' ' || correct_breed.super_breed) AS correct,
    quiz_question_v.filename AS image_filename
FROM quiz_answer
JOIN quiz_question_v ON quiz_question_v.quiz_id = quiz_answer.quiz_id
 AND quiz_question_v.question_id = quiz_answer.question_id
JOIN breed_v AS correct_breed ON correct_breed.id = quiz_question_v.breed_id
JOIN breed_v AS answer_breed ON answer_breed.id = quiz_answer.breed_id
WHERE quiz_answer.quiz_id = @quizId
  AND quiz_answer.user_session_id = @userSessionId
ORDER BY quiz_question_v.question_id
