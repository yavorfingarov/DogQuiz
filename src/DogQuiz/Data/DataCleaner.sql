--- UserSession.DeleteStale

DELETE FROM user_session
WHERE expiration < @now

--- Quiz.DeleteStale

DELETE FROM quiz
WHERE expiration < @now
  AND NOT EXISTS (
    SELECT 1
    FROM quiz_answer
    WHERE quiz_answer.quiz_id = quiz.id
  )

--- Db.IncrementalVacuum

PRAGMA incremental_vacuum;

--- Db.Optimize

PRAGMA optimize;
