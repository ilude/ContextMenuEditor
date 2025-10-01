# GitHub Copilot Instructions for Context Menu Editor

## Project Overview
This is a Windows WPF application for managing Windows context menu items. See #file:../PRD.md for complete product requirements.

## Development Philosophy

### SOLID Principles
Always follow SOLID programming principles:
- **Single Responsibility**: Each class should have one clear purpose
- **Open/Closed**: Open for extension, closed for modification
- **Liskov Substitution**: Derived classes must be substitutable for base classes
- **Interface Segregation**: Many specific interfaces over one general interface
- **Dependency Inversion**: Depend on abstractions, not concretions

### Simplicity First
- **Simple over complex**: Choose the straightforward solution
- **YAGNI**: You Aren't Gonna Need It - don't add features we don't need yet
- **KISS**: Keep It Simple, Stupid - avoid over-engineering
- **Readability counts**: Code is read more than written
- **Explicit is better than implicit**: Be clear about intent

## Technology Stack
- **Language**: C# 12 or latest
- **Framework**: WPF with .NET 8+ (or .NET Framework 4.8)
- **IDE**: Visual Studio Code
- **Architecture**: MVVM pattern for WPF

## Code Style Guidelines

### General
- Use meaningful, descriptive names for variables, methods, and classes
- Keep methods short and focused (ideally under 20 lines)
- Avoid nested conditionals - use early returns
- Prefer composition over inheritance
- Write self-documenting code; comments explain "why", not "what"

### C# Specifics
- Use modern C# features (pattern matching, null-coalescing, etc.)
- Prefer `var` for local variables when type is obvious
- Use string interpolation over concatenation
- Use async/await for I/O operations (registry access)
- Prefer readonly fields and properties
- Use nullable reference types

### WPF/XAML
- Follow MVVM pattern strictly
- ViewModels should not reference Views
- Use data binding over code-behind where possible
- Keep XAML clean and readable with proper formatting
- Use DataGrid for the main context menu list display
- Use ICommand for button actions

### Registry Operations
- Always validate registry paths before access
- Use try-catch for registry operations
- Create backups before destructive operations
- Check for admin privileges when needed
- Dispose registry keys properly (use `using` statements)

## Project Structure
```
ContextMenuEditor/
├── Models/           # Data models (context menu items, registry data)
├── ViewModels/       # MVVM ViewModels with INotifyPropertyChanged
├── Views/            # WPF Windows and UserControls
├── Services/         # Business logic (Registry, Backup, etc.)
├── Utilities/        # Helpers and extensions
└── Resources/        # Icons, styles, themes
```

## Key Features (Reference PRD.md)
1. Display context menus in a tabbed DataGrid interface
2. Enable/disable context menu items via checkboxes
3. Delete context menu items with confirmation
4. Export context menu list to text file
5. Multi-selection support for bulk operations
6. Registry backup before modifications
7. UAC elevation when needed

## Testing Approach
- Write testable code with dependency injection
- Separate business logic from UI logic
- Mock registry operations for unit tests
- Test edge cases (missing registry keys, invalid paths, etc.)

## Error Handling
- Never swallow exceptions silently
- Log errors appropriately
- Show user-friendly error messages
- Fail gracefully with proper cleanup
- Validate user input before registry operations

## Performance Considerations
- Load registry data asynchronously
- Use background workers for long operations
- Keep UI responsive during registry operations
- Implement progress indicators for bulk operations
- Minimize registry reads/writes

## Security
- Validate all registry paths to prevent injection
- Check permissions before registry modifications
- Request UAC elevation only when necessary
- Sanitize user input for custom context menu commands
- Create automatic backups before any destructive operation

## UI/UX Principles
- Keep interface simple and clean (reference CCleaner screenshot in PRD)
- Provide immediate visual feedback for actions
- Use standard Windows controls and patterns
- Support keyboard navigation
- Show confirmation dialogs for destructive actions
- Display clear status messages

## Git Workflow
- Write clear, descriptive commit messages
- Commit early and often
- Keep commits focused on single changes
- Reference issues/features in commit messages

## Remember
- Always reference #file:../PRD.md for product requirements
- When in doubt, choose the simpler solution
- Write code that others can easily understand and maintain
- Focus on v1.0 features first; avoid scope creep
