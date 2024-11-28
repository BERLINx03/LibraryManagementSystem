open System
open System.IO
type Book = {
    Title: string
    Author: string
    Genre: string
    IsAvailable: bool
}

type BorrowedBook = string * DateTime

type Member = {
    UserName: string
    BorrowedBooks: BorrowedBook list
}

let booksFile = "B:\\Development\\F# project\\books.txt"
let membersFile = "B:\\Development\\F# project\\members.txt"
let borrowingHistory = "B:\\Development\F# project\\borrowingHistory.txt"

//add members function
// Convert a Member to a file-friendly string
let memberToString (m: Member) =
    let books = 
        m.BorrowedBooks 
        |> List.map fst // Extract book names
        |> String.concat ", "
    $"{m.UserName} [{books}]"

// Parse a string back into a Member
let stringToMember (line: string) =
    let usernameEndIndex = line.IndexOf(" [")
    if usernameEndIndex < 0 then
        failwith "Invalid member format"
    else
        let username = line.Substring(0, usernameEndIndex)
        let booksPart = line.Substring(usernameEndIndex + 2).TrimEnd(']')
        let books = 
            if booksPart = "" then []
            else booksPart.Split(", ") |> List.ofArray |> List.map (fun title -> title, DateTime.MinValue)
        { UserName = username; BorrowedBooks = books }

// Add a member to the file
let addMemberToFile (filePath: string) (newMember: Member) =
    // Ensure the file exists
    if not (File.Exists filePath) then
        File.Create(filePath).Dispose() // Create an empty file

    // Read all members from the file
    let existingMembers =
        File.ReadAllLines(filePath)
        |> Array.map stringToMember
        |> Array.toList

    // Check if the username already exists
    if existingMembers |> List.exists (fun m -> m.UserName = newMember.UserName) then
        printfn $"This username already exists: %s{newMember.UserName}"
    else
        // Append the new member to the file
        let memberString = memberToString newMember
        File.AppendAllText(filePath, memberString + Environment.NewLine)
        printfn $"Member %s{newMember.UserName} has been added successfully."
        
//display members
// View all members
let viewAllMembers (filePath: string) =
    if not (File.Exists filePath) then
        printfn "Members file does not exist."
    else
        // Load members and print them
        let members =
            File.ReadAllLines(filePath)
            |> Array.map stringToMember

        if members.Length = 0 then
            printfn "No members found."
        else
            printfn "List of Members:"
            members
            |> Array.iter (fun m -> 
                let borrowedBooks = 
                    if m.BorrowedBooks |> List.isEmpty then "No books borrowed"
                    else m.BorrowedBooks |> List.map fst |> String.concat ", "
                printfn $"Username: {m.UserName}, Borrowed Books: {borrowedBooks}")

//////////////////////////////////////////////
// add update remove book

// Convert a Book to a file-friendly string
let bookToString (b: Book) =
    $"{b.Title}|{b.Author}|{b.Genre}|{b.IsAvailable}"

// Parse a string back into a Book
let stringToBook (line: string) =
    let parts = line.Split('|')
    if parts.Length <> 4 then
        failwith "Invalid book format"
    else
        {
            Title = parts[0]
            Author = parts[1]
            Genre = parts[2]
            IsAvailable = Boolean.Parse(parts[3])
        }

// Add a book to the file
let addBookToFile (filePath: string) (newBook: Book) =
    // Ensure the file exists
    if not (File.Exists filePath) then
        File.Create(filePath).Dispose() // Create an empty file

    // Read all books from the file
    let existingBooks =
        File.ReadAllLines(filePath)
        |> Array.map stringToBook
        |> Array.toList

    // Check if the book already exists by title
    if existingBooks |> List.exists (fun b -> b.Title = newBook.Title) then
        printfn $"The book with title '%s{newBook.Title}' already exists."
    else
        // Append the new book to the file
        let bookString = bookToString newBook
        File.AppendAllText(filePath, bookString + Environment.NewLine)
        printfn $"Book '%s{newBook.Title}' has been added successfully."

// Update a book in the file
let updateBookInFile (filePath: string) (updatedBook: Book) =
    if not (File.Exists filePath) then
        printfn "Books file does not exist."
    else
        // Read all books from the file
        let existingBooks =
            File.ReadAllLines(filePath)
            |> Array.map stringToBook
            |> Array.toList

        // Replace the old book entry with the updated one
        let updatedBooks =
            existingBooks
            |> List.map (fun b -> if b.Title = updatedBook.Title then updatedBook else b)

        // Write back the updated list of books
        let updatedContent = updatedBooks |> List.map bookToString |> String.concat Environment.NewLine
        File.WriteAllText(filePath, updatedContent)
        printfn $"Book '%s{updatedBook.Title}' has been updated successfully."

// Remove a book from the file
let removeBookFromFile (filePath: string) (title: string) =
    if not (File.Exists filePath) then
        printfn "Books file does not exist."
    else
        // Read all books from the file
        let existingBooks =
            File.ReadAllLines(filePath)
            |> Array.map stringToBook
            |> Array.toList

        // Filter out the book with the given title
        let remainingBooks =
            existingBooks
            |> List.filter (fun b -> b.Title <> title)

        // Write back the remaining books
        let updatedContent = remainingBooks |> List.map bookToString |> String.concat Environment.NewLine
        File.WriteAllText(filePath, updatedContent)
        printfn $"Book '%s{title}' has been removed successfully."

// search for a book
let searchBooks (booksFile: string) (query: string) =
    if not (File.Exists booksFile) then
        printfn "Books file does not exist."
    else
        // Load books and filter based on the query
        let books =
            File.ReadAllLines(booksFile)
            |> Array.map stringToBook
            |> Array.filter (fun b -> 
                b.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                b.Author.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                b.Genre.Contains(query, StringComparison.OrdinalIgnoreCase))

        // Print matching books
        if books |> Array.isEmpty then
            printfn $"No books found for query: %s{query}"
        else
            printfn "Search results:"
            books
            |> Array.iter (fun b -> printfn $"{b.Title} by {b.Author}, Genre: {b.Genre}, Available: {b.IsAvailable}")

///////////////////////////////////////////
// borrow functionality
// Borrow a book
let borrowBook (booksFile: string) (membersFile: string) (historyFile: string) (memberName: string) (bookTitle: string) =
    // Ensure all files exist
    if not (File.Exists booksFile && File.Exists membersFile) then
        printfn "Books or members file does not exist."
    else
        // Load books and members
        let books = 
            File.ReadAllLines(booksFile)
            |> Array.map stringToBook
            |> Array.toList

        let members =
            File.ReadAllLines(membersFile)
            |> Array.map stringToMember
            |> Array.toList

        // Find the book and member
        match List.tryFind (fun b -> b.Title = bookTitle && b.IsAvailable) books, 
              List.tryFind (fun m -> m.UserName = memberName) members with
        | Some book, Some m ->
            // Update the book's availability
            let updatedBooks = 
                books
                |> List.map (fun b -> if b.Title = book.Title then { b with IsAvailable = false } else b)

            // Update the member's borrowed books
            let updatedMember =
                { m with BorrowedBooks = (bookTitle, DateTime.Now) :: m.BorrowedBooks }

            let updatedMembers =
                members
                |> List.map (fun m -> if m.UserName = m.UserName then updatedMember else m)

            // Write back the updated books and members
            let updatedBooksContent = updatedBooks |> List.map bookToString |> String.concat Environment.NewLine
            File.WriteAllText(booksFile, updatedBooksContent)

            let updatedMembersContent = updatedMembers |> List.map memberToString |> String.concat Environment.NewLine
            File.WriteAllText(membersFile, updatedMembersContent)

            // Add to history file
            let historyEntry = $"{memberName} borrowed {bookTitle} on {DateTime.Now}"
            File.AppendAllText(historyFile, historyEntry + Environment.NewLine)

            printfn $"Member '%s{memberName}' borrowed the book '%s{bookTitle}'."
        | None, _ -> printfn "Book not available or member not found."
        | _, None -> printfn "Member not found."

// Return a book
let returnBook (booksFile: string) (membersFile: string) (historyFile: string) (memberName: string) (bookTitle: string) =
    // Ensure all files exist
    if not (File.Exists booksFile && File.Exists membersFile) then
        printfn "Books or members file does not exist."
    else
        // Load books and members
        let books = 
            File.ReadAllLines(booksFile)
            |> Array.map stringToBook
            |> Array.toList

        let members =
            File.ReadAllLines(membersFile)
            |> Array.map stringToMember
            |> Array.toList

        // Find the book and member
        match List.tryFind (fun b -> b.Title = bookTitle) books, 
              List.tryFind (fun m -> m.UserName = memberName) members with
        | Some book, Some m ->
            // Update the book's availability
            let updatedBooks = 
                books
                |> List.map (fun b -> if b.Title = book.Title then { b with IsAvailable = true } else b)

            // Update the member's borrowed books
            let updatedMember =
                { m with BorrowedBooks = m.BorrowedBooks |> List.filter (fun (title, _) -> title <> bookTitle) }

            let updatedMembers =
                members
                |> List.map (fun m -> if m.UserName = m.UserName then updatedMember else m)

            // Write back the updated books and members
            let updatedBooksContent = updatedBooks |> List.map bookToString |> String.concat Environment.NewLine
            File.WriteAllText(booksFile, updatedBooksContent)

            let updatedMembersContent = updatedMembers |> List.map memberToString |> String.concat Environment.NewLine
            File.WriteAllText(membersFile, updatedMembersContent)

            // Add to history file
            let historyEntry = $"{memberName} returned {bookTitle} on {DateTime.Now}"
            File.AppendAllText(historyFile, historyEntry + Environment.NewLine)

            printfn $"Member '%s{memberName}' returned the book '%s{bookTitle}'."
        | None, _ -> printfn "Book not found."
        | _, None -> printfn "Member not found."
////////////////////////////////////
// creating a report for the existing books
let listAvailableBooks (booksFile: string) =
    if not (File.Exists booksFile) then
        printfn "Books file does not exist."
    else
        // Load books and filter only available ones
        let availableBooks =
            File.ReadAllLines(booksFile)
            |> Array.map stringToBook
            |> Array.filter (fun b -> b.IsAvailable)

        // Print available books
        if availableBooks |> Array.isEmpty then
            printfn "No available books at the moment."
        else
            printfn "Available books:"
            availableBooks
            |> Array.iter (fun b -> printfn $"{b.Title} by {b.Author}, Genre: {b.Genre}")
////////////////////////////////////
// creates a list of the transactions happened
let printHistory (historyFile: string) =
    if not (File.Exists historyFile) then
        printfn "History file does not exist."
    else
        // Read and print history file contents
        let history = File.ReadAllLines(historyFile)
        if history.Length = 0 then
            printfn "The borrowing history is empty."
        else
            printfn "Borrowing History:"
            history
            |> Array.iter (fun line -> printfn $"{line}")
///////////////////////////////////
// the main function

[<EntryPoint>]
let main argv =
    let rec menu () =
        printfn "\nLibrary Management System"
        printfn "1. Add a new member"
        printfn "2. Add a new book"
        printfn "3. Update a book"
        printfn "4. Remove a book"
        printfn "5. Borrow a book"
        printfn "6. Return a book"
        printfn "7. Search for a book"
        printfn "8. List available books"
        printfn "9. Print borrowing history"
        printfn "0. Exit"
        printf "Enter your choice: "
        match Console.ReadLine() with
        | "1" ->
            printf "Enter username: "
            let username = Console.ReadLine()
            let m = { UserName = username; BorrowedBooks = [] }
            addMemberToFile membersFile m
            menu ()
        | "2" ->
            printf "Enter book title: "
            let title = Console.ReadLine()
            printf "Enter book author: "
            let author = Console.ReadLine()
            printf "Enter book genre: "
            let genre = Console.ReadLine()
            let book = { Title = title; Author = author; Genre = genre; IsAvailable = true }
            addBookToFile booksFile book
            menu ()
        | "3" ->
            printf "Enter the title of the book to update: "
            let title = Console.ReadLine()
            printf "Enter new author: "
            let author = Console.ReadLine()
            printf "Enter new genre: "
            let genre = Console.ReadLine()
            printf "Is the book available? (true/false): "
            let isAvailable = Boolean.Parse(Console.ReadLine())
            let updatedBook = { Title = title; Author = author; Genre = genre; IsAvailable = isAvailable }
            updateBookInFile booksFile updatedBook
            menu ()
        | "4" ->
            printf "Enter the title of the book to remove: "
            let title = Console.ReadLine()
            removeBookFromFile booksFile title
            menu ()
        | "5" ->
            printf "Enter your username: "
            let username = Console.ReadLine()
            printf "Enter the book title to borrow: "
            let title = Console.ReadLine()
            borrowBook booksFile membersFile borrowingHistory username title
            menu ()
        | "6" ->
            printf "Enter your username: "
            let username = Console.ReadLine()
            printf "Enter the book title to return: "
            let title = Console.ReadLine()
            returnBook booksFile membersFile borrowingHistory username title
            menu ()
        | "7" ->
            printf "Enter search query (title, author, or genre): "
            let query = Console.ReadLine()
            searchBooks booksFile query
            menu ()
        | "8" ->
            listAvailableBooks booksFile
            menu ()
        | "9" ->
            printHistory borrowingHistory
            menu ()
        | "10" ->
            viewAllMembers membersFile
            menu ()
        | "0" ->
            printfn "Exiting the system. Goodbye!"
            0
        | _ ->
            printfn "Invalid choice. Please try again."
            menu ()
    
    menu ()
