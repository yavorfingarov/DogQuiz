--- Breed.GetSummary

SELECT
    breed_v.super_breed,
    breed_v.sub_breed,
    COUNT(*) AS image_count,
    IIF(breed_v.questions != 0, ROUND(CAST(breed_v.correct_answers AS REAL) / breed_v.questions * 100, 2), NULL) AS recognizability
FROM breed_v
JOIN image ON image.breed_id = breed_v.id
GROUP BY breed_v.id

--- QuizCount.Get

SELECT val
FROM quiz_count

--- QuizResult.GetCount

SELECT COUNT(*)
FROM quiz_result

--- QuizResult.GetAverageScore

SELECT ROUND(AVG(score), 2)
FROM quiz_result
