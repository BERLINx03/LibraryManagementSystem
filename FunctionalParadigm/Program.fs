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

let booksFile = "books.txt"
let membersFile = "members.txt"
let borrowingHistory = "borrowingHistory.txt"

//add members function
let memberToString (m: Member) =
    let books = 
        m.BorrowedBooks 
        |> List.map fst
        |> String.concat ", "
    $"{m.UserName} [{books}]"

// Parse a string back into a Member
let stringToMember (line: string) =
    try
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
    with
    | :? ArgumentException as ex ->
        printfn "Argument exception: %s" ex.Message
        { UserName = ""; BorrowedBooks = [] } // Return a default member object
    | :? FormatException as ex ->
        printfn "Format exception: %s" ex.Message
        { UserName = ""; BorrowedBooks = [] }
    | ex ->
        printfn "Unexpected error: %s" ex.Message
        { UserName = ""; BorrowedBooks = [] }

// Add a member to the file
let addMemberToFile (filePath: string) (newMember: Member) =
    try
        if not (File.Exists filePath) then
            File.Create(filePath).Dispose() // Create an empty file if it doesn't exist

        let existingMembers =
            try
                File.ReadAllLines(filePath)
                |> Array.map stringToMember
                |> Array.toList
            with
            | ex ->
                printfn "Error reading members: %s" ex.Message
                [] // Return an empty list if there's an error

        if existingMembers |> List.exists (fun m -> m.UserName = newMember.UserName) then
            printfn "Username '%s' already exists." newMember.UserName
        else
            let memberString = memberToString newMember
            try
                File.AppendAllText(filePath, memberString + Environment.NewLine)
                printfn "Member '%s' added successfully." newMember.UserName
            with
            | ex ->
                printfn "Error writing to file: %s" ex.Message
    with
    | :? IOException as ex ->
        printfn "File operation failed: %s" ex.Message
    | ex ->
        printfn "Unexpected error: %s" ex.Message


//display members
let viewAllMembers (filePath: string) =
    try
        if not (File.Exists filePath) then
            printfn "Members file does not exist."
        else
            let members =
                try
                    File.ReadAllLines(filePath)
                    |> Array.map stringToMember
                with
                | ex ->
                    printfn "Error reading members: %s" ex.Message
                    [||] // Return an empty array

            if members.Length = 0 then
                printfn "No members found."
            else
                printfn "List of Members:"
                members
                |> Array.iter (fun m -> 
                    let borrowedBooks = 
                        if m.BorrowedBooks |> List.isEmpty then "No books borrowed"
                        else m.BorrowedBooks |> List.map fst |> String.concat ", "
                    printfn "Username: %s, Borrowed Books: %s" m.UserName borrowedBooks)
    with
    | ex ->
        printfn "Unexpected error: %s" ex.Message

// add update remove book
let bookToString (b: Book) =
    $"{b.Title}|{b.Author}|{b.Genre}|{b.IsAvailable}"

// Parse a string back into a Book
let stringToBook (line: string) =
    try
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
    with
    | :? FormatException as ex ->
        printfn "Error parsing book: %s" ex.Message
        { Title = ""; Author = ""; Genre = ""; IsAvailable = false } // Return a default book object
    | ex ->
        printfn "Unexpected error: %s" ex.Message
        { Title = ""; Author = ""; Genre = ""; IsAvailable = false }


// Add a book to the file
let addBookToFile (filePath: string) (newBook: Book) =
    try
        if not (File.Exists filePath) then
            File.Create(filePath).Dispose() // Create file if it doesn't exist

        let existingBooks =
            try
                File.ReadAllLines(filePath)
                |> Array.map stringToBook
                |> Array.toList
            with
            | ex ->
                printfn "Error reading books: %s" ex.Message
                [] // Return an empty list

        if existingBooks |> List.exists (fun b -> b.Title = newBook.Title) then
            printfn "Book '%s' already exists." newBook.Title
        else
            let bookString = bookToString newBook
            try
                File.AppendAllText(filePath, bookString + Environment.NewLine)
                printfn "Book '%s' added successfully." newBook.Title
            with
            | ex ->
                printfn "Error writing to file: %s" ex.Message
    with
    | :? IOException as ex ->
        printfn "File operation failed: %s" ex.Message
    | ex ->
        printfn "Unexpected error: %s" ex.Message


let updateBookInFile (filePath: string) (updatedBook: Book) =
    if not (File.Exists filePath) then
        printfn "Books file does not exist."
    else
        // Read all books from the file
        let existingBooks =
            File.ReadAllLines(filePath)
            |> Array.map stringToBook
            |> Array.toList

        let updatedBooks =
            existingBooks
            |> List.map (fun b -> if b.Title = updatedBook.Title then updatedBook else b)

        let updatedContent = updatedBooks |> List.map bookToString |> String.concat Environment.NewLine
        File.WriteAllText(filePath, updatedContent)
        printfn $"Book '%s{updatedBook.Title}' has been updated successfully."

let removeBook (booksFilePath: string) (membersFilePath: string) (bookTitle: string) =
    try
        if not (File.Exists booksFilePath) then
            printfn "Books file does not exist."
        elif not (File.Exists membersFilePath) then
            printfn "Members file does not exist."
        else
            // Read all members and check if any member has borrowed the book
            let members =
                try
                    File.ReadAllLines(membersFilePath)
                    |> Array.map stringToMember
                    |> Array.toList
                with
                | ex ->
                    printfn "Error reading members: %s" ex.Message
                    []

            let membersWithBook =
                members
                |> List.filter (fun m -> m.BorrowedBooks |> List.exists (fun (title, _) -> title = bookTitle))

            if membersWithBook |> List.isEmpty then
                // If no member has the book, proceed to remove it from the books file
                let books =
                    try
                        File.ReadAllLines(booksFilePath)
                        |> Array.map stringToBook
                        |> Array.toList
                    with
                    | ex ->
                        printfn "Error reading books: %s" ex.Message
                        []

                let updatedBooks = books |> List.filter (fun b -> b.Title <> bookTitle)

                if List.length books = List.length updatedBooks then
                    printfn "Book '%s' does not exist in the library." bookTitle
                else
                    try
                        let updatedContent = updatedBooks |> List.map bookToString |> String.concat Environment.NewLine
                        File.WriteAllText(booksFilePath, updatedContent)
                        printfn "Book '%s' has been removed successfully." bookTitle
                    with
                    | ex ->
                        printfn "Error writing to books file: %s" ex.Message
            else
                // If the book is borrowed, list the members who have it
                printfn "Book '%s' is currently borrowed by the following member(s):" bookTitle
                membersWithBook
                |> List.iter (fun m -> printfn "- %s" m.UserName)

    with
    | :? IOException as ex ->
        printfn "File operation failed: %s" ex.Message
    | ex ->
        printfn "Unexpected error: %s" ex.Message


// search for a book
let searchBooks (booksFile: string) (query: string) =
    if not (File.Exists booksFile) then
        printfn "Books file does not exist."
    else
        let books =
            File.ReadAllLines(booksFile)
            |> Array.map stringToBook
            |> Array.filter (fun b -> 
                b.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                b.Author.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                b.Genre.Contains(query, StringComparison.OrdinalIgnoreCase))

        if books |> Array.isEmpty then
            printfn $"No books found for query: %s{query}"
        else
            printfn "Search results:"
            books
            |> Array.iter (fun b -> printfn $"{b.Title} by {b.Author}, Genre: {b.Genre}, Available: {b.IsAvailable}")

// Borrow a book
let borrowBook (booksFile: string) (membersFile: string) (historyFile: string) (memberName: string) (bookTitle: string) =
    if not (File.Exists booksFile && File.Exists membersFile) then
        printfn "Books or members file does not exist."
    else
        let books = 
            File.ReadAllLines(booksFile)
            |> Array.map stringToBook
            |> Array.toList

        let members =
            File.ReadAllLines(membersFile)
            |> Array.map stringToMember
            |> Array.toList

        match List.tryFind (fun b -> b.Title = bookTitle && b.IsAvailable) books, 
              List.tryFind (fun m -> m.UserName = memberName) members with
        | Some book, Some m ->
            let updatedBooks = 
                books
                |> List.map (fun b -> if b.Title = book.Title then { b with IsAvailable = false } else b)

            let updatedMember =
                { m with BorrowedBooks = (bookTitle, DateTime.Now) :: m.BorrowedBooks }

            let updatedMembers =
                members
                |> List.map (fun m -> if m.UserName = memberName then updatedMember else m)

            let updatedBooksContent = updatedBooks |> List.map bookToString |> String.concat Environment.NewLine
            File.WriteAllText(booksFile, updatedBooksContent)

            let updatedMembersContent = updatedMembers |> List.map memberToString |> String.concat Environment.NewLine
            File.WriteAllText(membersFile, updatedMembersContent)

            let historyEntry = $"{memberName} borrowed {bookTitle} on {DateTime.Now}"
            File.AppendAllText(historyFile, historyEntry + Environment.NewLine)

            printfn $"Member '%s{memberName}' borrowed the book '%s{bookTitle}'."
        | None, _ -> printfn "Book not available or member not found."
        | _, None -> printfn "Member not found."

// Return a book
let returnBook (booksFile: string) (membersFile: string) (historyFile: string) (memberName: string) (bookTitle: string) =
    if not (File.Exists booksFile && File.Exists membersFile) then
        printfn "Books or members file does not exist."
    else
        let books = 
            File.ReadAllLines(booksFile)
            |> Array.map stringToBook
            |> Array.toList

        let members =
            File.ReadAllLines(membersFile)
            |> Array.map stringToMember
            |> Array.toList

        match List.tryFind (fun b -> b.Title = bookTitle) books, 
              List.tryFind (fun m -> m.UserName = memberName) members with
        | Some book, Some m ->
            let updatedBooks = 
                books
                |> List.map (fun b -> if b.Title = book.Title then { b with IsAvailable = true } else b)

            let updatedMember =
                { m with BorrowedBooks = m.BorrowedBooks |> List.filter (fun (title, _) -> title <> bookTitle) }

            let updatedMembers =
                members
                |> List.map (fun m -> if m.UserName = memberName then updatedMember else m)

            let updatedBooksContent = updatedBooks |> List.map bookToString |> String.concat Environment.NewLine
            File.WriteAllText(booksFile, updatedBooksContent)

            let updatedMembersContent = updatedMembers |> List.map memberToString |> String.concat Environment.NewLine
            File.WriteAllText(membersFile, updatedMembersContent)

            let historyEntry = $"{memberName} returned {bookTitle} on {DateTime.Now}"
            File.AppendAllText(historyFile, historyEntry + Environment.NewLine)

            printfn $"Member '%s{memberName}' returned the book '%s{bookTitle}'."
        | None, _ -> printfn "Book not found."
        | _, None -> printfn "Member not found."
// creating a report for the existing books
let listAvailableBooks (booksFile: string) =
    if not (File.Exists booksFile) then
        printfn "Books file does not exist."
    else
        let availableBooks =
            File.ReadAllLines(booksFile)
            |> Array.map stringToBook
            |> Array.filter (fun b -> b.IsAvailable)

        if availableBooks |> Array.isEmpty then
            printfn "No available books at the moment."
        else
            printfn "Available books:"
            availableBooks
            |> Array.iter (fun b -> printfn $"{b.Title} by {b.Author}, Genre: {b.Genre}")
// creates a list of the transactions happened
let printHistory (historyFile: string) =
    if not (File.Exists historyFile) then
        printfn "History file does not exist."
    else
        let history = File.ReadAllLines(historyFile)
        if history.Length = 0 then
            printfn "The borrowing history is empty."
        else
            printfn "Borrowing History:"
            history
            |> Array.iter (fun line -> printfn $"{line}")
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
        printfn "10. View all members"
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
            removeBook booksFile title
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
