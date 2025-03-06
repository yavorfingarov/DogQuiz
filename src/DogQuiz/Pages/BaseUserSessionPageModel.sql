--- UserSession.Create

INSERT INTO user_session (id, expiration)
VALUES (@userSessionId, @expiration)

--- UserSession.Update

UPDATE user_session
SET expiration = @expiration
WHERE id = @userSessionId
