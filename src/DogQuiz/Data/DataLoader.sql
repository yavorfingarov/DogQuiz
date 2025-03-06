--- Breed.Any

SELECT 1
FROM breed
LIMIT 1

--- SuperBreed.Create

INSERT INTO super_breed (display_name)
VALUES (@superBreed)
RETURNING id

--- SubBreed.Create

INSERT INTO sub_breed (super_breed_id, display_name)
VALUES (@superBreedId, @subBreed)
RETURNING id

--- Breed.Create

INSERT INTO breed (id, super_breed_id, sub_breed_id, questions, correct_answers)
VALUES (@breedId, @superBreedId, @subBreedId, 0, 0)

--- Image.Create

INSERT INTO image (breed_id, filename, file_version, source_url)
VALUES (@breedId, @filename, 1, @imageUrl)
