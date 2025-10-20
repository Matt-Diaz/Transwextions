# Transwextions
A simple purchase tracking application that stores transactions in USD and retrieves them allowing the ability to convert into other currencies using the U.S. Treasury Reporting Rates of Exchange API.
Built with Blazor Server, Entity Framework Core, and SQLite, designed for easy setup (No manual database/server configuration required).

## Resources
- **Radzen Blazor Components**
  - https://blazor.radzen.com/get-started?theme=material3

- **U.S. Treasury Reporting Rates of Exchange API**
  - https://fiscaldata.treasury.gov/datasets/treasury-reporting-rates-exchange/treasury-reporting-rates-of-exchange#api-quick-guide

- **Avatar Images**
  - https://www.freepik.com/free-vector/round-avatars-set-with-faces-people-comic-portraits-happy-social-media-users_22676039.htm#fromView=search&page=1&position=46&uuid=e2cbea21-0243-44a1-8b1d-d1ce54503d23&query=Avatar+pack
## Features

### Purchase Management
- Create and store  transactions with:
  - **Description** (required, max 50 characters)
  - **Transaction date** (valid date format)
  - **Purchase amount** (positive, rounded to nearest cent)
  - **Auto-generated unique identifier (GUID)**
- Persist all transactions in a **local SQLite database** (no setup required)
- **Automatic migration update** on first run — database is created and updated automatically

### Currency Conversion
- Retrieve stored transactions in any supported currency
- Integrates with the **U.S. Treasury Reporting Rates of Exchange API**
- Selects the **most recent exchange rate ≤ purchase date** within a 6-month window
- Displays:
  - Original USD amount
  - Exchange rate used
  - Rate date
  - Converted amount (rounded to 2 decimals)
- Returns a clear error if no rate exists within the 6-month window

### Business Logic & Validation
- Enforces data integrity using **Entity Framework Core constraints**:
  - Description length limit
  - Positive amounts only
- All arithmetic uses **`decimal`** types for precise currency math

### Technical Features
- Built with **Blazor Server (.NET 9)** and **Entity Framework Core**
- Uses **SQLite** as an embedded, file-based data store
- **Auto-migration** and **Directory Creation** ensure zero configuration
- Supports **dependency injection**

### UI
- Modern responsive UI built with **Radzen Blazor Components**
- Create and view transactions through intuitive forms and data tables
- Currency selector with real-time conversion results
- Validation messages and toast notifications for user feedback

### Unit Tests
This project includes a comprehensive suite of **unit tests** built with **NUnit** and **FluentAssertions**.  
Tests are organized by feature (e.g., *Services*, *Helpers*) and follow the standard  
**Arrange → Mock → Act → Assert** pattern for clarity and maintainability.
