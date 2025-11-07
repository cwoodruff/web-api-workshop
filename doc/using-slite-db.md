---
order: 3
icon: container
---
# Installing and Setting Up SQLite Database

For this workshop, we will be using SQLite as our database. SQLite is a lightweight, file-based database that is easy to set up and use.
## Installing SQLite
1. Go to the [SQLite Download Page](https://www.sqlite.org/download.html).
2. Download the appropriate precompiled binary for your operating system.
3. Follow the installation instructions provided on the SQLite website for your specific OS.
4. Verify the installation by opening a terminal or command prompt and typing:
   ```bash
   sqlite3 --version
   ```
   You should see the version number of SQLite printed in the terminal.
5. Congratulations! You have successfully installed SQLite on your machine.
6. ## Next Steps
7. You can now create a new SQLite database for your project. To create a new database, open a terminal and run:
   ```bash
   sqlite3 mydatabase.db
   ```
   This command will create a new SQLite database file named `mydatabase.db` in your current directory.
8. You can start executing SQL commands to create tables and insert data into your database.
9. For more information on how to use SQLite, refer to the [SQLite Documentation](https