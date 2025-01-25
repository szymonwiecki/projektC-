// script.js
let token = "";

document.addEventListener("DOMContentLoaded", () => {
    // Logowanie
    const loginForm = document.getElementById("login-form");
    const loginMessage = document.getElementById("login-message");

    loginForm.addEventListener("submit", async (e) => {
        e.preventDefault();
        const username = document.getElementById("username").value;
        const password = document.getElementById("password").value;

        try {
            const response = await fetch("https://localhost:7106/api/Auth/login", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ username, password })
            });

            if (response.ok) {
                const data = await response.json();
                loginMessage.textContent = "Token generated: " + data.token;
                token = data.token;
            } else {
                loginMessage.textContent = "Login failed!";
            }
        } catch (err) {
            loginMessage.textContent = "Error during login!";
        }
    });

    // Autoryzacja
    const authorizeBtn = document.getElementById("authorize-btn");
    const authorizeMessage = document.getElementById("authorize-message");

    authorizeBtn.addEventListener("click", () => {
        const tokenInput = document.getElementById("token-input").value.trim();
        if (tokenInput) {
            token = tokenInput;
            authorizeMessage.textContent = "Token set successfully!";
        } else {
            authorizeMessage.textContent = "Please enter a valid token.";
        }
    });

    // Fetch Books
    const getBooksBtn = document.getElementById("get-books-btn");
    const booksList = document.getElementById("books-list");

    getBooksBtn.addEventListener("click", async () => {
        try {
            const response = await fetch("https://localhost:7106/api/Books", {
                method: "GET",
                headers: { 
                    "Authorization": `Bearer ${token}`,
                    "accept": "text/plain"        
                }
            });

            if (response.ok) {
                const books = await response.json();
                booksList.innerHTML = books.map(book =>
                    `<li>ID: ${book.id}, Title: ${book.title} by ${book.author} (${book.publishedYear}), ${book.genre}</li>`
                ).join("");
            } else {
                booksList.innerHTML = "Failed to fetch books.";
            }
        } catch (err) {
            booksList.innerHTML = "Error while fetching books.";
        }
    });

    //Get book by id
    document.getElementById("get-book-form").addEventListener("submit", async (event) => {
        event.preventDefault();
    
        const bookId = document.getElementById("get-book-id").value;
        const resultDiv = document.getElementById("get-book-result");
    
        try {
            const response = await fetch(`https://localhost:7106/api/Books/${bookId}`, {
                method: "GET",
                headers: {
                    "Authorization": `Bearer ${token}`,
                    "Content-Type": "application/json"
                }
            });
    
            if (response.ok) {
                const book = await response.json();
                resultDiv.innerHTML = `
                    <strong>Book Details:</strong><br>
                    <strong>ID:</strong> ${book.id}<br>
                    <strong>Title:</strong> ${book.title}<br>
                    <strong>Author:</strong> ${book.author}<br>
                    <strong>Year:</strong> ${book.publishedYear}<br>
                    <strong>Genre:</strong> ${book.genre}
                `;
                resultDiv.style.color = "black";
            } else if (response.status === 404) {
                const errorData = await response.json();
                resultDiv.textContent = `Error: ${errorData.message}`;
                resultDiv.style.color = "red";
            } else {
                const errorData = await response.json();
                resultDiv.textContent = `Error: ${errorData.message}`;
                resultDiv.style.color = "red";
            }
        } catch (error) {
            console.error("Error fetching book:", error);
            resultDiv.textContent = "Failed to fetch book. Check the console for details.";
            resultDiv.style.color = "red";
        }
    });

    // Add Book
    document.getElementById("add-book-form").addEventListener("submit", async (event) => {
        event.preventDefault();
    
        // Pobieramy dane z formularza
        const id = document.getElementById("add-id").value;
        const title = document.getElementById("add-title").value;
        const author = document.getElementById("add-author").value;
        const year = document.getElementById("add-year").value;
        const genre = document.getElementById("add-genre").value;
    
        const messageDiv = document.getElementById("add-book-message");
    
            // Wysyłamy żądanie POST do backendu
            const response = await fetch("https://localhost:7106/api/Books", {
                method: "POST",
                headers: {
                    "Authorization": `Bearer ${token}`,
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({
                    id: parseInt(id), // Przesyłamy pełny obiekt książki
                    title: title,
                    author: author,
                    publishedYear: parseInt(year), // Konwertujemy rok na liczbę
                    genre: genre
                })
            });
    
            if (response.ok) {
                const book = await response.json();
                messageDiv.textContent = `Book added successfully! ID: ${book.id}`;
                messageDiv.style.color = "green";
    
                // Opcjonalnie czyścimy formularz
                document.getElementById("add-book-form").reset();
            } else if (response.status === 400) {
                const errorData = await response.json();
                messageDiv.textContent = `Error: ${errorData.message}`;
                messageDiv.style.color = "red";
            } else {
                const errorData = await response.json();
                messageDiv.textContent = `Error: ${errorData.message}`;
                messageDiv.style.color = "red";
            }
    });
    
    

    // Edit Book
    document.getElementById("edit-book-form").addEventListener("submit", async (event) => {
        event.preventDefault();
    
        // Pobieramy dane z formularza
        const id = document.getElementById("edit-id").value;
        const title = document.getElementById("edit-title").value;
        const author = document.getElementById("edit-author").value;
        const year = document.getElementById("edit-year").value;
        const genre = document.getElementById("edit-genre").value;
    
        const messageDiv = document.getElementById("edit-book-message");
    
            // Wysyłamy żądanie PUT do backendu
            const response = await fetch(`https://localhost:7106/api/Books/${id}`, {
                method: "PUT",
                headers: {
                    "Authorization": `Bearer ${token}`,
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({
                    id: parseInt(id), // ID musi być takie samo w URL i w body
                    title: title,
                    author: author,
                    publishedYear: parseInt(year), // Konwertujemy na liczbę
                    genre: genre
                })
            });
    
            if (response.ok) {
                messageDiv.textContent = "Book updated successfully!";
                messageDiv.style.color = "green";
            } else if (response.status === 400) {
                const errorData = await response.json();
                messageDiv.textContent = `Error: ${errorData.message}`;
                messageDiv.style.color = "red";
            } else if (response.status === 404) {
                const errorData = await response.json();
                messageDiv.textContent = `Error: ${errorData.message}`;
                messageDiv.style.color = "red";
            } else {
                const errorData = await response.json();
                messageDiv.textContent = `Error: ${errorData.message}`;
                messageDiv.style.color = "red";
            }
        
    });
    // Delete Book
    const deleteBookForm = document.getElementById("delete-book-form");
    const deleteBookMessage = document.getElementById("delete-book-message");

    deleteBookForm.addEventListener("submit", async (e) => {
        e.preventDefault();
        const id = parseInt(document.getElementById("delete-id").value, 10);

        try {
            const response = await fetch(`https://localhost:7106/api/Books/${id}`, {
                method: "DELETE",
                headers: {
                    "Authorization": `Bearer ${token}`,
                }
            });

            if (response.ok) {
                deleteBookMessage.textContent = "Book deleted successfully!";
            } else {
                deleteBookMessage.textContent = "Failed to delete book.";
            }
        } catch (err) {
            deleteBookMessage.textContent = "Error while deleting book.";
        }
    });
});
