# Groups Management Module

A comprehensive Blazor-based group management system that interfaces with Oracle database (ZOCLINIC schema). This module provides full CRUD operations for managing organizational groups.

## Features

- **List View**: Browse and search all groups with pagination
- **Profile/Edit View**: View and modify group details including address information
- **Create View**: Add new groups to the system
- **Search Functionality**: Search groups by name, ID, city, or state
- **Pagination**: Navigate through large datasets efficiently

## Database Schema

This module connects to the Oracle `ZOCLINIC.GROUPS_OLD` table with the following structure:

```sql
TABLE: ZOCLINIC.GROUPS_OLD
- GROUP_ID (NUMBER, Primary Key)
- GROUP_CODE (VARCHAR2(4))
- GROUP_NAME (VARCHAR2(100), Required)
- DESCRIPTION (VARCHAR2(100))
- CREATED_DATE (DATE)
- LAST_UPDATE (DATE)
- COMPANY_ID (NUMBER)
- ADDRESS_1 (VARCHAR2(100))
- ADDRESS_2 (VARCHAR2(100))
- CITY (VARCHAR2(50))
- STATE (VARCHAR2(20))
- ZIP (VARCHAR2(20))
```

## Setup Instructions

### 1. Database Configuration

Add the Oracle connection string to your `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "ClinicDbConnection": "Data Source=your_oracle_server;User Id=your_username;Password=your_password;"
  }
}
```

### 2. Register DbContext

In your `Program.cs` or `Startup.cs`, register the `ClinicDbContext`:

```csharp
using LLMCraftedBlazorSamples.Shared.Data;
using Oracle.EntityFrameworkCore;

// Add DbContext with Oracle provider
builder.Services.AddDbContext<ClinicDbContext>(options =>
    options.UseOracle(builder.Configuration.GetConnectionString("ClinicDbConnection")));
```

### 3. Install Required NuGet Packages

```bash
dotnet add package Oracle.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore
```

## Pages and Routes

### Groups List (`/Groups` or `/Groups/list`)
- Displays all groups in a paginated table
- Search by Group Name, Group ID, City, or State
- Clicking a row navigates to the profile page
- "NEW GROUP" button to create new groups

### Group Profile (`/Groups/profile?id={groupId}`)
- Edit group details (name, address, contact information)
- View creation and last update timestamps
- Save changes or cancel

### Create Group (`/Groups/create`)
- Form to create a new group
- Required fields: Group Name
- Optional fields: Group Code, Email, Address details

## Components Used

- **QuickGrid**: For tabular data display (not used in final implementation)
- **EditForm**: For data entry and validation
- **DataAnnotationsValidator**: For model validation
- **StatCard**: For displaying statistics (from Shared components)
- **FieldDisplay**: For consistent field rendering (from Shared components)

## File Structure

```
/Groups/
├── Groups.razor              # Main list view
├── GroupProfile.razor        # Edit/profile view
├── CreateGroup.razor         # Create new group view
└── README.md                 # This file

/Shared/
├── Models/
│   └── Group.cs              # Group entity model
└── Data/
    └── ClinicDbContext.cs    # Oracle DbContext
```

## Usage Examples

### Navigating to Groups List
```
https://yourapp.com/Groups
```

### Viewing a Specific Group
```
https://yourapp.com/Groups/profile?id=92815
```

### Creating a New Group
```
https://yourapp.com/Groups/create
```

## Styling

The Groups module follows the legacy UI styling with:
- Yellow/cream colored backgrounds (#FFFACD)
- Navy blue table headers (#00305A)
- Orange accent color for buttons (#FF6B35)
- Alternating row colors (white and light yellow)

## Future Enhancements

- [ ] Group Users management (`/Groups/users?id={groupId}`)
- [ ] Export to Excel functionality
- [ ] Advanced filtering options
- [ ] Group status management (Active/Inactive)
- [ ] Audit logging for changes
- [ ] Bulk operations (delete, update)

## Dependencies

- **Microsoft.EntityFrameworkCore** - ORM framework
- **Oracle.EntityFrameworkCore** - Oracle provider for EF Core
- **Microsoft.AspNetCore.Components** - Blazor framework
- **Bootstrap 5** - UI styling
- **Bootstrap Icons** - Icon library

## Integration with Main Application

To add the Groups module to your navigation:

```razor
<li class="nav-item">
    <a class="nav-link" href="/Groups">
        <i class="bi bi-people-fill me-2"></i>Groups
    </a>
</li>
```

## Troubleshooting

### Issue: "Table or view does not exist"
**Solution**: Ensure your Oracle user has access to the ZOCLINIC schema and GROUPS_OLD table.

### Issue: "DbContext not registered"
**Solution**: Make sure you've registered `ClinicDbContext` in your dependency injection container.

### Issue: "Oracle client not found"
**Solution**: Install Oracle Data Access Components (ODAC) on your server.

## License

This module is part of the LLMCraftedBlazorSamples repository and is distributed under the MIT License.

## Support

For issues or questions, please open an issue on the GitHub repository.
