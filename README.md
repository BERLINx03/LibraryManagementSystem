# Library Management System (F#)

This project is a simple library management system implemented in F#. It provides functionalities for managing books and members, handling borrowing and returning of books, and keeping track of the transaction history.

## Features

1. **Member Management**: 
   - Add new members to the system.
   - View all members and their borrowed books.
   
2. **Book Management**: 
   - Add new books to the library.
   - Update book details (e.g., title, author, genre, availability).
   - Remove books from the library.
   
3. **Borrowing and Returning Books**: 
   - Borrow books from the library.
   - Return borrowed books.
   - Automatically update book availability and member borrowing records.
   
4. **History Management**: 
   - Record all borrow and return transactions.
   - View the full borrowing history, including the member, book, and transaction date.
   
5. **Search Functionality**: 
   - Search for books by title, author, or genre.
   - Display available books in the library.

## Files

The system operates on three main files that store data:

- `books.txt`: Stores information about the books in the library.
- `members.txt`: Stores information about library members and the books they have borrowed.
- `borrowingHistory.txt`: Logs all borrowing and returning activities.

## File Format

### books.txt
Each line represents a book and follows this format:
```
Title|Author|Genre|IsAvailable
```

Where:
- `Title`: The title of the book.
- `Author`: The author of the book.
- `Genre`: The genre of the book.
- `IsAvailable`: A boolean value indicating if the book is available for borrowing (`true` or `false`).

### members.txt
Each line represents a member and follows this format:
```
UserName [Book1, Book2, ...]
```
Where:
- `UserName`: The username of the member.
- `Book1, Book2, ...`: A comma-separated list of books the member has borrowed.

### borrowingHistory.txt
Each line represents a transaction (borrow or return) and follows this format:
```
MemberName borrowed/returned BookTitle MM-DD-YYYY HH:MM:SS:   
