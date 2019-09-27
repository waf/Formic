# Formic

Autogenerate an admin UI from an EF Core DbContext at runtime.

## Status

Just a proof-of-concept at this point.  It supports:

- basic CRUD operations
- Navigating / editing foreign keys
- Customizing the display / edit templates of properties, using razor

It's hard-coded to a demo DbContext (FormicDbContext) but it should be straightforward to parameterize.

## Running it

1. Clone the repo
2. Run the `sass` grunt task to build the frontend css
3. Build and run the project. On initial start it will create a sample `formicdb` database in `(localdb)\mssqllocaldb`
