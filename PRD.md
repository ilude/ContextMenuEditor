# Context Menu Editor - Product Requirements Document

## Overview
Context Menu Editor is a Windows GUI application that allows users to easily manage, edit, and customize context menu items and startup programs through an intuitive interface.

**Status**: ✅ **v1.1 COMPLETED** - Windows Startup Management with UI Improvements  
**v1.0**: ✅ COMPLETED - See Implementation Summary section below

## Product Vision
To provide Windows users with a simple, safe, and comprehensive tool for managing their Windows customizations (context menus and startup programs) without requiring direct registry editing knowledge.

## Target Audience
- **Primary**: Power users and system administrators who want to customize their Windows experience
- **Secondary**: Software developers who need to manage context menu entries for their applications
- **Tertiary**: General users who want to clean up cluttered context menus

## Core Features

### 1. Context Menu Discovery and Display ✅ **IMPLEMENTED**
- ✅ **File Context Menus**: Displays all context menu items from `HKEY_CLASSES_ROOT\*\shell` and `shellex\ContextMenuHandlers`
- ✅ **Directory Context Menus**: Shows items from `Directory\shell`, `Directory\Background\shell`, and handlers
- ✅ **Drive Context Menus**: Lists items from `Drive\shell` and handlers
- ✅ **Unified Single-View Interface**: Single DataGrid with Type column (File/Directory/Drive/Background) instead of tabs - simpler, more efficient
- ✅ **Grid Display**: Sortable DataGrid with columns: Enabled (checkbox), Type, Menu Text, Key, Publisher, File path
- ✅ **Smart Deduplication**: Shows one entry per program while tracking all registry locations (HKEY_CLASSES_ROOT and HKEY_CURRENT_USER)
- ✅ **Windows System Filtering**: Automatically filters out built-in Windows programs from System32 using environment variables
- ✅ **Resource String Resolution**: Uses P/Invoke (SHLoadIndirectString) to resolve display names from DLL resources

### 2. Context Menu Management ✅ **IMPLEMENTED**
- ✅ **Enable/Disable Items**: 
  - Toggle via checkbox in the grid with INotifyPropertyChanged for instant visual feedback
  - Select item(s) and use Enable/Disable buttons
  - Uses registry `LegacyDisable` value to preserve entries while making them inactive
  - Applies to **all registry locations** for deduplicated items (both HKEY_CLASSES_ROOT and HKEY_CURRENT_USER)
  - Visual feedback: disabled items shown in gray italic text
- ✅ **Delete Items**: 
  - Permanently removes registry keys from all tracked locations
  - Confirmation dialog with warning message
  - Removes item from UI on successful deletion
- ✅ **View Details**: DataGrid displays registry key, program name, menu text, publisher, and command path
- ✅ **Export to .REG File**: Context-specific "Export Context Menus" button
  - Exports all context menu items to a standard Windows .REG file
  - Includes all registry values, subkeys, and command entries
  - Restorable by double-clicking the .REG file
  - Timestamped filename (e.g., `ContextMenuBackup_20251001_143052.reg`)
- ✅ **Quote Stripping**: Clean display of command paths without leading/trailing quotes via StripQuotesConverter
- ✅ **Multi-Selection**: DataGrid supports Ctrl+Click and Shift+Click for selecting multiple items
- ✅ **UAC Elevation**: Application requires administrator privileges via app.manifest for registry modifications

### 3. Custom Context Menu Creation ⏳ **DEFERRED TO v2.0**
- ⏳ **Add New Items**: Create custom context menu entries (not implemented in v1.0)
- ⏳ **Edit Existing Items**: Modify properties of custom entries (not implemented in v1.0)

**Rationale**: v1.0 focused on core management functionality (view, enable/disable, delete, backup). Custom menu creation requires additional UI complexity and validation, suitable for a future release.

### 3.1 Advanced Features ⏳ **PLANNED FOR v2.0**
- Custom icons from files or system icon library
- Submenu support for nested items
- Conditional display based on file types
- Working directory specification
- Keyboard shortcuts
- Custom context menu creation and editing

### 5. Windows Startup Management ✅ **IMPLEMENTED (v1.1)**
- ✅ **Separate Tab Interface**: Dedicated "Startup Programs" tab alongside "Context Menus" tab
- ✅ **Startup Discovery**: Discovers startup items from:
  - `HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run`
  - `HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\Run`
  - `HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\RunOnce`
  - `HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\RunOnce`
- ✅ **Enable/Disable Startup Items**: 
  - Uses Windows 10+ `StartupApproved\Run` mechanism for disable tracking
  - Visual feedback with disabled items shown in gray italic
  - Requires admin rights for system-level items
- ✅ **Delete Startup Items**: Permanently removes from Run/RunOnce with confirmation
- ✅ **Export to .REG File**: Context-specific "Export Startup Items" button for registry backup
- ✅ **Grid Display**: Columns: Enabled (checkbox), Name, Publisher, Command, Location (UserRun/SystemRun/etc)
- ✅ **Windows System Filtering**: Filters out built-in Windows startup programs
- ✅ **Publisher Detection**: Auto-detects software vendors from common path patterns
- ✅ **Quote Stripping**: Clean display of command paths without leading/trailing quotes

### 4. Essential Features ✅ **IMPLEMENTED**
- ✅ **Column Sorting**: DataGrid supports sorting by clicking any column header
- ✅ **Column Width Management**: Menu Text auto-sizes to content, Command column takes remaining space
- ✅ **Registry Export**: Context-specific export buttons ("Export Context Menus" / "Export Startup Items") for .REG file backup
- ✅ **Safety Features**: 
  - Confirmation dialogs for delete operations
  - All registry values preserved in .REG backup files
  - Error handling with user-friendly messages
- ✅ **Administrator Mode**: app.manifest requires UAC elevation on startup for full registry access
- ✅ **Dark Mode**: Full dark/light theme system with Windows 10/11 title bar integration
  - Dark mode as default
  - Toggle button in header (shows opposite mode: "Light" in dark, "Dark" in light)
  - Uses P/Invoke DwmSetWindowAttribute for native title bar theming
- ✅ **Custom Application Icon**: Resources/app.ico configured in .csproj
- ⏳ **Quick Search**: Standard Ctrl+F search functionality (UI supports, not explicitly implemented)
- ⏳ **Refresh**: Reload command exists in ViewModel but may need verification

### 4.1 Advanced Features ⏳ **PLANNED FOR FUTURE VERSIONS**
- Import/Export of custom configurations between systems
- Real-time registry monitoring
- Undo/Redo functionality
- Advanced filtering and saved searches
- Multi-selection batch operations (UI supports, commands may need update)

## Technical Requirements ✅ **IMPLEMENTED**

### Platform Requirements
- ✅ **Operating System**: Windows 10/11 (build 17763+ for dark title bar)
- ✅ **Framework**: .NET 8 Windows WPF
- ✅ **Architecture**: MVVM (Model-View-ViewModel) pattern with dependency injection
- ✅ **Language**: C# 12 with nullable reference types enabled
- ✅ **IDE**: Visual Studio Code (not Visual Studio)
- ✅ **Privileges**: Programmatic elevation with UAC prompt (app.manifest with asInvoker, elevation check in App.xaml.cs)
- ✅ **Dependencies**: 
  - Microsoft.Win32.Registry v5.0.0
  - P/Invoke: shlwapi.dll (SHLoadIndirectString), dwmapi.dll (DwmSetWindowAttribute)
- ✅ **Elevation Helper**: Cross-platform elevation detection using WindowsIdentity/WindowsPrincipal

### Performance Requirements
- **Startup Time**: Application should launch within 2 seconds
- **Registry Operations**: Context menu changes should apply within 1 second
- **Memory Usage**: Maximum 100MB RAM during normal operation
- **Responsiveness**: UI should remain responsive during all operations

### Security Requirements
- **Registry Safety**: All registry modifications must be reversible
- **Backup System**: Automatic backup creation before any destructive operations
- **Input Validation**: Strict validation of user inputs to prevent system damage
- **Privilege Escalation**: Secure handling of UAC prompts when administrator access is required

## User Interface Requirements

### Main Window Layout ✅ **IMPLEMENTED** with Tabs for Separate Features
- ✅ **Tab-Based Navigation**: Separate tabs for independent features
  - **Context Menus Tab**: Single unified DataGrid with Type column (File/Directory/Drive/Background)
  - **Startup Programs Tab**: Dedicated view for Windows startup management
  - **Design Decision**: Tabs for completely different features (context menus vs. startup), unified view within each feature
- ✅ **Data Grid View (Context Menus)**: Displays:
  - **Enabled Column**: Checkbox with INotifyPropertyChanged binding for instant toggle
  - **Type Column**: Shows File, Directory, Drive, or Background
  - **Menu Text Column**: User-friendly display name (what appears in context menu), auto-width
  - **Key Column**: Registry key identifier
  - **Publisher Column**: Software vendor/creator (auto-detected)
  - **Command Column**: Path to executable with quotes stripped, takes remaining space, minimum 150px width
- ✅ **Data Grid View (Startup Programs)**: Displays:
  - **Enabled Column**: Checkbox with INotifyPropertyChanged binding
  - **Name Column**: Startup item name
  - **Publisher Column**: Software vendor (auto-detected)
  - **Command Column**: Executable path with quotes stripped
  - **Location Column**: UserRun, SystemRun, UserRunOnce, or SystemRunOnce
- ✅ **Action Buttons Panel** (Right side, dark-styled border):
  - **Enable**: Removes LegacyDisable (context menus) or StartupApproved flag (startup)
  - **Disable**: Adds LegacyDisable or StartupApproved flag, grays out row
  - **Delete**: Permanently removes registry keys with confirmation dialog
  - **Export Context Menus** / **Export Startup Items**: Context-specific labels for .REG file export
  - **Refresh**: Reloads items from registry
- ✅ **Header**: Theme toggle button (🌙 Light/Dark) - no explanatory text for cleaner UI
- ✅ **Status Bar**: Shows count of loaded items

### Simplified UI Principles
- **No complex tree views**: Flat list in a sortable data grid
- **Direct manipulation**: Click checkbox to toggle, select and use buttons for actions
- **Minimal chrome**: Focus on the data grid with essential controls only
- **Clear visual feedback**: Blue highlight for selected items, gray for disabled items
- **Multi-selection support**: Allow bulk operations on multiple items

### Key User Flows
1. **View Context Menus**: User clicks tab → sees flat list of all items in grid
2. **Disable Unwanted Item**: User unchecks checkbox OR selects row and clicks Disable button
3. **Delete Item**: User selects row(s) → clicks Delete → confirms in dialog
4. **Enable Item**: User checks checkbox OR selects row and clicks Enable button
5. **Export List**: User clicks "Save to text file..." → chooses location → saves
6. **Sort and Find**: User clicks column headers to sort, uses Ctrl+F to search

### Accessibility
- **Keyboard Navigation**: Full application functionality accessible via keyboard
- **Screen Reader Support**: Proper ARIA labels and screen reader compatibility
- **High Contrast**: Support for Windows high contrast themes
- **Font Scaling**: Respect system font size settings

## Success Metrics
- **User Adoption**: Track application downloads and active users
- **Feature Usage**: Monitor which features are used most frequently
- **Error Rates**: Minimize system errors and failed operations
- **User Satisfaction**: Collect feedback on ease of use and effectiveness

## Risk Mitigation
- **Registry Corruption**: Comprehensive backup and validation systems
- **System Compatibility**: Extensive testing across Windows versions
- **User Error**: Clear warnings and confirmation dialogs for destructive actions
- **Performance Impact**: Efficient registry operations to avoid system slowdown

## Future Enhancements (Out of Scope for v1.0)
- **Network Context Menus**: Support for network drive and UNC path context menus
- **Shell Extension Management**: Broader shell extension management beyond context menus
- **Scripting Support**: PowerShell/batch script integration for advanced users
- **Cloud Sync**: Synchronize context menu configurations across devices
- **Plugin System**: Allow third-party plugins for specialized context menu types

## Success Criteria for v1.0 ✅ **ACHIEVED**
- ✅ Successfully discovers and displays context menu items from registry for files, directories, and drives
- ✅ Users can safely enable/disable context menu items via checkbox or buttons with visual feedback
- ✅ Reliable backup functionality to .REG file format (more robust than text export)
- ⏳ Create custom context menu items (deferred to v2.0 - scope focused on management first)
- ✅ System stability maintained with proper error handling and UAC elevation
- ✅ Registry operations work reliably with admin privileges
- ✅ Application follows SOLID principles and "simple over complex" philosophy

## Timeline and Milestones
- **Phase 1**: Core discovery and display functionality (4 weeks)
- **Phase 2**: Basic editing and management features (3 weeks)
- **Phase 3**: Custom menu creation and advanced features (4 weeks)
- **Phase 4**: Testing, polish, and documentation (2 weeks)
- **Total Duration**: 13 weeks for v1.0 release

---

## v1.0 Implementation Summary

### What Was Built
Context Menu Editor v1.0 successfully delivers core context menu management functionality with a clean, intuitive interface. The application went through iterative development with continuous user feedback, resulting in several design improvements over the original plan.

### Key Design Decisions

1. **Single View Instead of Tabs**
   - **Original Plan**: Separate tabs for Files, Directories, Drives, Background, IE, Scheduled Tasks
   - **Implementation**: Single unified DataGrid with Type column
   - **Rationale**: Simpler, more efficient, allows cross-type sorting and filtering

2. **Backup to .REG Instead of Text**
   - **Original Plan**: Export to text file for documentation
   - **Implementation**: Full .REG file export with all registry values
   - **Rationale**: Provides actual restoration capability, more useful than documentation

3. **Dark Mode as Default**
   - **Not in Original Plan**: Theme system with dark/light toggle
   - **Implementation**: Complete theming with Windows 10/11 title bar integration
   - **Rationale**: Modern UI preference, enhanced user experience

4. **Smart Deduplication**
   - **Challenge**: Same programs registered in multiple registry locations
   - **Solution**: Show one UI entry while tracking all registry locations
   - **Benefit**: Cleaner UI, but operations apply to all instances

5. **Windows System Filtering**
   - **Challenge**: Too many built-in Windows items cluttering the view
   - **Solution**: Filter out programs from Windows/System32 using environment variables
   - **Benefit**: Shows only third-party programs users want to manage

6. **Resource String Resolution**
   - **Challenge**: Menu text showing as "@shell32.dll,-8506" or file paths
   - **Solution**: P/Invoke SHLoadIndirectString to resolve DLL resource strings
   - **Benefit**: User-friendly display names matching actual context menus

### Technology Stack
- **Framework**: .NET 8 Windows WPF
- **Pattern**: MVVM with ViewModelBase, RelayCommand, INotifyPropertyChanged
- **Registry**: Microsoft.Win32.Registry, RegistryService with async operations
- **P/Invoke**: SHLoadIndirectString (shlwapi.dll), DwmSetWindowAttribute (dwmapi.dll)
- **Theme System**: ThemeManager singleton with dynamic resource binding
- **Security**: UAC elevation via app.manifest (requireAdministrator)

### Project Structure
```
ContextMenuEditor/
├── Models/                    # ContextMenuItem, RegistryLocation
├── ViewModels/                # MainViewModel, ViewModelBase
├── Views/                     # MainWindow.xaml
├── Services/                  # IRegistryService, RegistryService
├── Utilities/                 # RelayCommand, ThemeManager, ResourceStringResolver, WindowHelper, ThemeLabelConverter
├── Resources/                 # app.ico
├── app.manifest              # UAC elevation configuration
└── App.xaml                  # Theme resources and application startup
```

### Code Metrics (Commit 932c0ce)
- **Files**: 19 files
- **Lines of Code**: 1,979 insertions
- **Repository**: git main branch with proper .gitignore

### What Works
✅ Registry discovery from HKEY_CLASSES_ROOT and HKEY_CURRENT_USER  
✅ Enable/disable with LegacyDisable registry value  
✅ Delete with confirmation and multi-location support  
✅ Backup to .REG file with all values and subkeys  
✅ Dark/light theme with system title bar integration  
✅ Smart deduplication showing one entry per program  
✅ Windows system program filtering  
✅ Resource string resolution (P/Invoke)  
✅ Visual feedback (gray italic for disabled items)  
✅ UAC elevation for registry access  
✅ Custom application icon  
✅ Column width management (auto-size, flexible)  

### What's Deferred
⏳ Custom context menu creation (Add/Edit new items)  
⏳ Refresh command verification  
⏳ Multi-selection batch operations (UI supports, commands need work)  
⏳ Quick search (Ctrl+F) implementation  
⏳ Advanced features: IE menus, Scheduled Tasks, real-time monitoring  

### Development Philosophy Followed
- **SOLID Principles**: Single responsibility, dependency injection, interface segregation
- **Simple Over Complex**: Avoided over-engineering, chose straightforward solutions
- **YAGNI**: Didn't add features not needed for v1.0
- **KISS**: Kept code clean and readable
- **User Feedback Driven**: Iterated based on actual usage feedback

## Conclusion
Context Menu Editor v1.0 successfully provides Windows users with a clean, powerful tool to manage their context menus efficiently and safely, eliminating the need for direct registry editing while maintaining full control over the user experience. The application delivers on its core promise with a focus on simplicity, safety, and usability.

---

## v1.1 Implementation Summary

### New Feature: Windows Startup Management ✅ **COMPLETED**

**Goal**: Expand the application to manage Windows startup programs, following the same architectural patterns and UI principles as the context menu management.

**Implementation Details**:
1. **Separate Tab**: Added "Startup Programs" tab alongside "Context Menus" tab
   - Each tab represents a completely different feature
   - Maintains design principle: unified view within each feature, tabs for separate features
2. **New Models**: `StartupItem` with properties: Name, Command, Publisher, Location, IsEnabled, RegistryPath
   - `StartupLocation` enum: UserRun, SystemRun, UserRunOnce, SystemRunOnce
3. **New Service**: `StartupService` implementing `IStartupService` for registry operations
   - Discovers items from Run and RunOnce keys (User and System level)
   - Enable/Disable using Windows 10+ `StartupApproved` mechanism
   - Delete operations with confirmation
   - Export to .REG file format
4. **New ViewModel**: `StartupViewModel` following same pattern as `MainViewModel`
   - ObservableCollection of startup items
   - Enable/Disable/Delete/Export/Refresh commands
   - Status messages and loading states
5. **New View**: `StartupView` UserControl with same layout as context menu view
   - DataGrid with columns: Enabled, Name, Publisher, Command, Location
   - Action buttons panel on the right
   - Status bar at bottom
6. **Consistent Patterns**: Reuses existing infrastructure
   - ViewModelBase for INotifyPropertyChanged
   - RelayCommand for commands
   - Same styling and theming from App.xaml
   - Similar enable/disable/delete workflows

**Registry Locations**:
- **User Run**: `HKCU\Software\Microsoft\Windows\CurrentVersion\Run`
- **System Run**: `HKLM\Software\Microsoft\Windows\CurrentVersion\Run`
- **User RunOnce**: `HKCU\Software\Microsoft\Windows\CurrentVersion\RunOnce`
- **System RunOnce**: `HKLM\Software\Microsoft\Windows\CurrentVersion\RunOnce`
- **Disable Tracking**: `HKCU/HKLM\...\Explorer\StartupApproved\Run` (Windows 10+)

**Elevation Enhancement**:
- **Programmatic UAC Elevation**: Changed from manifest `requireAdministrator` to `asInvoker`
  - `ElevationHelper.cs`: Cross-platform elevation detection using WindowsIdentity/WindowsPrincipal
  - `App.xaml.cs`: Checks elevation on startup, prompts for UAC if needed
  - **Benefit**: Allows `dotnet run` to work without running IDE as admin
  - Inspired by: https://anthonysimmon.com/building-wpf-app-elevation-uac/

**UI Improvements**:
- **Context-Specific Export Buttons**: 
  - "Export Context Menus" on Context Menus tab
  - "Export Startup Items" on Startup Programs tab
  - Previous generic "Backup Registry Entries" was unclear
- **Consistent Column Naming**: 
  - Changed "File" to "Command" on Context Menus tab
  - Both tabs now use "Command" for executable paths
- **Quote Stripping**:
  - `StripQuotesConverter.cs`: IValueConverter to remove leading/trailing quotes
  - Applied to Command columns on both tabs
  - Cleaner visual display of file paths

**Project Structure Additions**:
```
ContextMenuEditor/
├── Models/
│   └── StartupItem.cs                    # Startup program data model
├── ViewModels/
│   └── StartupViewModel.cs               # MVVM ViewModel for startup tab
├── Views/
│   └── StartupView.xaml/.xaml.cs         # Startup Programs tab UI
├── Services/
│   ├── IStartupService.cs                # Startup service interface
│   └── StartupService.cs                 # Startup registry operations
└── Utilities/
    ├── ElevationHelper.cs                # UAC elevation detection/launching
    └── StripQuotesConverter.cs           # WPF value converter for quotes
```

**Code Commits** (v1.1):
1. `7c25e9a` - (MINOR) Add programmatic elevation and remove requireAdministrator
2. `f8b0e6a` - Rename backup buttons for clarity: Export Context/Startup
3. `63ff368` - Improve UI: Rename columns to Command and strip quotes

**Testing Status**: ✅ Code Complete, ⏳ Awaiting Runtime Testing
- ✅ Code compiles successfully
- ✅ All changes committed and pushed
- ⏳ Runtime testing with UAC elevation prompt
- ⏳ Verify enable/disable functionality on actual startup items
- ⏳ Test delete operations
- ⏳ Validate export/restore workflow with .REG files

**Version**: v1.1.0 (MINOR version bump per semantic versioning)