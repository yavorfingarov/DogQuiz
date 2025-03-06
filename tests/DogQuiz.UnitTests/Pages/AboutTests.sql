--- _Test.Breed.Update

UPDATE breed
SET questions = @questions,
    correct_answers = @correctAnswers
WHERE id = @breedId

--- _Test.QuizCount.Update

UPDATE quiz_count
SET val = @val
