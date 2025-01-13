document.addEventListener("DOMContentLoaded", async () => {
    const booksList = document.getElementById("books-list");

    try {
        const response = await fetch("/api/books");
        if (!response.ok) throw new Error("Failed to fetch books");
        const books = await response.json();

        booksList.innerHTML = books.map(book =>
            `<li>${book.title} by ${book.author} (${book.publishedYear})</li>`
        ).join("");
    } catch (error) {
        booksList.innerHTML = `<li>Error: ${error.message}</li>`;
    }
});
