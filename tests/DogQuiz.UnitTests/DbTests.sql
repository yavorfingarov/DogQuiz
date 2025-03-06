--- _Test.Db.Schema

SELECT sql
FROM sqlite_schema
WHERE sql IS NOT NULL
ORDER BY rootpage, sql
