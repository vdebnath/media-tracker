CREATE TABLE MediaItems (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Title TEXT NOT NULL,
    Type TEXT NOT NULL, 
    Status TEXT NOT NULL,
    Notes TEXT,
    DateAdded TEXT NOT NULL
);

-- INSERT 3 media items
INSERT INTO MediaItems (Title, Type, Status, Notes, DateAdded)
VALUES ('A Game of Thrones', 'Book', 'InProgress', 'Winter is Coming', '06-15-2026'),
       ('DBZ Movie 8: Broly – The Legendary Super Saiyan', 'Movie', 'Done', 'Kakarot!', '06-15-2026'),
       ('Assassination Classroom', 'Show', 'InProgress', 'Kill Teacher', '06-15-2026');

-- SELECT all items
SELECT * FROM MediaItems;

-- SELECT only Books
SELECT * FROM MediaItems
WHERE Type = 'Book';

-- UPDATE status of one item
UPDATE MediaItems
SET Status = 'Complete'
WHERE Title = 'Assassination Classroom';

-- DELETE one item by Id
DELETE FROM MediaItems
WHERE Id = 3;

-- Final SELECT to confirm changes
SELECT * FROM MediaItems;