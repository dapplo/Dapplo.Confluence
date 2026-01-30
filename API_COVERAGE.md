# Confluence API Coverage Analysis

This document provides a comprehensive analysis of the Dapplo.Confluence library's coverage of the Atlassian Confluence REST API. It identifies missing functionality, obsolete endpoints, and areas for improvement.

## Table of Contents

- [Overview](#overview)
- [Current Implementation](#current-implementation)
- [Missing Functionality](#missing-functionality)
- [Obsolete or Deprecated Features](#obsolete-or-deprecated-features)
- [Feature Requests](#feature-requests)

## Overview

Dapplo.Confluence is a REST-based Confluence client that currently supports several key domains:
- **Content Domain**: Page and content management
- **Space Domain**: Space operations
- **User Domain**: User operations and watch functionality
- **Group Domain**: Group operations
- **Attachment Domain**: Attachment management
- **Misc Domain**: Miscellaneous operations (pictures, system info)

The library supports both Confluence Server and Confluence Cloud with appropriate differences handled where necessary.

## Current Implementation

### Content Domain
**File**: [src/Dapplo.Confluence/ContentExtensions.cs](src/Dapplo.Confluence/ContentExtensions.cs)

**Implemented Methods**:
- ✅ Create content (pages, blog posts)
- ✅ Get content by ID
- ✅ Get content by title
- ✅ Get children (child pages)
- ✅ Get history
- ✅ Search content (CQL-based)
- ✅ Update content
- ✅ Delete content
- ✅ Move content (Cloud only)
- ✅ Copy content (Cloud only)
- ✅ Get labels
- ✅ Add labels
- ✅ Delete labels
- ✅ Export page as PDF (via FlyingPDF)

### Space Domain
**File**: [src/Dapplo.Confluence/SpaceExtensions.cs](src/Dapplo.Confluence/SpaceExtensions.cs)

**Implemented Methods**:
- ✅ Create space
- ✅ Create private space
- ✅ Get space by key
- ✅ Get all spaces (with filtering)
- ✅ Get space contents
- ✅ Update space
- ✅ Delete space

### User Domain
**File**: [src/Dapplo.Confluence/UserExtensions.cs](src/Dapplo.Confluence/UserExtensions.cs)

**Implemented Methods**:
- ✅ Get current user
- ✅ Get anonymous user
- ✅ Get user by username/account ID
- ✅ Get user's group memberships
- ✅ Add/remove/check content watchers
- ✅ Add/remove/check label watchers (Cloud only)
- ✅ Add/remove/check space watchers

### Group Domain
**File**: [src/Dapplo.Confluence/GroupExtensions.cs](src/Dapplo.Confluence/GroupExtensions.cs)

**Implemented Methods**:
- ✅ Get all groups
- ✅ Get group members by group ID

### Attachment Domain
**File**: [src/Dapplo.Confluence/AttachmentExtensions.cs](src/Dapplo.Confluence/AttachmentExtensions.cs)

**Implemented Methods**:
- ✅ Attach files to content
- ✅ Get attachments for content
- ✅ Get attachment content (download)
- ✅ Update attachment metadata
- ✅ Update attachment data
- ✅ Delete attachment

### Misc Domain
**File**: [src/Dapplo.Confluence/MiscExtensions.cs](src/Dapplo.Confluence/MiscExtensions.cs)

**Implemented Methods**:
- ✅ Get pictures (avatars, etc.)
- ✅ Get system info (Cloud only)

## Missing Functionality

### 1. Content Domain - Missing Features

**Reference**: [Confluence Cloud REST API - Content](https://developer.atlassian.com/cloud/confluence/rest/v1/api-group-content/)

#### Content Restrictions and Permissions
- ❌ **Get content restrictions** - `GET /rest/api/content/{id}/restriction`
  - Get information about all restrictions for a piece of content
  - **Use Case**: Check who can view/edit specific pages
  
- ❌ **Update content restrictions** - `PUT /rest/api/content/{id}/restriction`
  - Update existing restrictions on content
  - **Use Case**: Modify page permissions programmatically
  
- ❌ **Add content restrictions** - `POST /rest/api/content/{id}/restriction`
  - Add new restrictions to content
  - **Use Case**: Restrict page access to specific users/groups
  
- ❌ **Delete content restrictions** - `DELETE /rest/api/content/{id}/restriction/byGroup/{groupName}` and similar
  - Remove restrictions from content
  - **Use Case**: Open up previously restricted content

#### Content Properties
- ❌ **Get content properties** - `GET /rest/api/content/{id}/property`
  - Retrieve custom properties attached to content
  - **Use Case**: Store and retrieve custom metadata on pages
  
- ❌ **Create content property** - `POST /rest/api/content/{id}/property`
  - Add custom property to content
  - **Use Case**: Tag pages with custom metadata
  
- ❌ **Update content property** - `PUT /rest/api/content/{id}/property/{key}`
  - Update existing content property
  - **Use Case**: Modify custom metadata
  
- ❌ **Delete content property** - `DELETE /rest/api/content/{id}/property/{key}`
  - Remove content property
  - **Use Case**: Clean up obsolete metadata

#### Content Versions
- ❌ **Get content version** - `GET /rest/api/content/{id}/version/{versionNumber}`
  - Get a specific version of content
  - **Use Case**: View historical versions of pages
  
- ❌ **Get content version history** - `GET /rest/api/content/{id}/version`
  - Get all versions for content
  - **Use Case**: Track page change history
  
- ❌ **Restore content version** - `POST /rest/api/content/{id}/version`
  - Restore a previous version of content
  - **Use Case**: Revert unwanted changes

#### Content Descendants
- ❌ **Get descendants** - `GET /rest/api/content/{id}/descendant`
  - Get all descendant content (not just immediate children)
  - **Use Case**: Get entire page tree under a parent
  
- ❌ **Get descendants by type** - `GET /rest/api/content/{id}/descendant/{type}`
  - Get descendants filtered by type (page, comment, attachment)
  - **Use Case**: Find all attachments in a page tree

#### Comments
- ❌ **Get comments for content** - `GET /rest/api/content/{id}/child/comment`
  - Retrieve comments on a page
  - **Use Case**: Export or analyze page discussions
  
- ❌ **Create comment** - `POST /rest/api/content` (with type=comment)
  - Add comments to pages
  - **Use Case**: Automated commenting on pages
  
- ❌ **Update comment** - `PUT /rest/api/content/{id}` (for comment)
  - Edit existing comments
  - **Use Case**: Correct or update automated comments
  
- ❌ **Delete comment** - `DELETE /rest/api/content/{id}` (for comment)
  - Remove comments
  - **Use Case**: Clean up test comments

#### Content Body Conversion
- ❌ **Convert content body** - `POST /rest/api/contentbody/convert/{to}`
  - Convert content between formats (storage, view, export_view, styled_view, editor)
  - **Use Case**: Convert wiki markup to HTML for display

#### Content Templates
- ❌ **Get content templates** - `GET /rest/api/template`
  - Get available content templates
  - **Use Case**: List templates for creating standardized pages
  
- ❌ **Get template** - `GET /rest/api/template/{contentTemplateId}`
  - Get a specific template
  - **Use Case**: Retrieve template for content creation
  
- ❌ **Create template** - `POST /rest/api/template`
  - Create new content template
  - **Use Case**: Define reusable page structures
  
- ❌ **Update template** - `PUT /rest/api/template`
  - Update existing template
  - **Use Case**: Modify template definitions
  
- ❌ **Delete template** - `DELETE /rest/api/template/{contentTemplateId}`
  - Remove template
  - **Use Case**: Clean up unused templates

### 2. Space Domain - Missing Features

**Reference**: [Confluence Cloud REST API - Space](https://developer.atlassian.com/cloud/confluence/rest/v1/api-group-space/)

#### Space Permissions
- ❌ **Get space permissions** - `GET /rest/api/space/{spaceKey}/permission`
  - Get permission settings for a space
  - **Use Case**: Audit space access rights
  
- ❌ **Add space permissions** - `POST /rest/api/space/{spaceKey}/permission`
  - Add permissions to a space
  - **Use Case**: Grant access to users/groups
  
- ❌ **Remove space permissions** - `DELETE /rest/api/space/{spaceKey}/permission/{id}`
  - Remove permissions from a space
  - **Use Case**: Revoke access rights

#### Space Properties
- ❌ **Get space properties** - `GET /rest/api/space/{spaceKey}/property`
  - Get custom properties for a space
  - **Use Case**: Retrieve space-level metadata
  
- ❌ **Create space property** - `POST /rest/api/space/{spaceKey}/property`
  - Add custom property to space
  - **Use Case**: Store space configuration
  
- ❌ **Update space property** - `PUT /rest/api/space/{spaceKey}/property/{key}`
  - Update space property
  - **Use Case**: Modify space metadata
  
- ❌ **Delete space property** - `DELETE /rest/api/space/{spaceKey}/property/{key}`
  - Remove space property
  - **Use Case**: Clean up obsolete properties

#### Space Content
- ❌ **Get space content by type** - `GET /rest/api/space/{spaceKey}/content/{type}`
  - Get content of specific type in a space
  - **Use Case**: List all blog posts or pages in a space

#### Space Settings
- ❌ **Get space settings** - `GET /rest/api/space/{spaceKey}/settings`
  - Get space configuration settings
  - **Use Case**: Retrieve space preferences
  
- ❌ **Update space settings** - `PUT /rest/api/space/{spaceKey}/settings`
  - Update space settings
  - **Use Case**: Configure space behavior

#### Space Theme
- ❌ **Get space theme** - `GET /rest/api/space/{spaceKey}/theme`
  - Get the theme for a space
  - **Use Case**: Retrieve space styling
  
- ❌ **Set space theme** - `PUT /rest/api/space/{spaceKey}/theme`
  - Set or update space theme
  - **Use Case**: Apply custom styling to space
  
- ❌ **Reset space theme** - `DELETE /rest/api/space/{spaceKey}/theme`
  - Reset space to default theme
  - **Use Case**: Remove custom styling

### 3. User Domain - Missing Features

**Reference**: [Confluence Cloud REST API - User](https://developer.atlassian.com/cloud/confluence/rest/v1/api-group-users/)

#### User Search
- ❌ **Search users** - `GET /rest/api/user/search` (with query parameter)
  - Search for users by name or email
  - **Use Case**: Find users for mentions or permissions

#### User Properties
- ❌ **Get user properties** - `GET /rest/api/user/properties`
  - Get all user properties
  - **Use Case**: Retrieve user preferences
  
- ❌ **Get user property** - `GET /rest/api/user/properties/{key}`
  - Get specific user property
  - **Use Case**: Read user setting
  
- ❌ **Update user property** - `PUT /rest/api/user/properties/{key}`
  - Update user property
  - **Use Case**: Store user preferences
  
- ❌ **Delete user property** - `DELETE /rest/api/user/properties/{key}`
  - Remove user property
  - **Use Case**: Clean up settings

#### Bulk User Operations
- ❌ **Get bulk users** - `GET /rest/api/user/bulk`
  - Get multiple users by account IDs
  - **Use Case**: Efficiently retrieve user details for multiple accounts

### 4. Group Domain - Missing Features

**Reference**: [Confluence Cloud REST API - Groups](https://developer.atlassian.com/cloud/confluence/rest/v1/api-group-group/)

#### Group Management
- ❌ **Get group by name** - `GET /rest/api/group/by-name`
  - Get specific group details by name
  - **Use Case**: Retrieve group information
  
- ❌ **Get group by id** - `GET /rest/api/group/by-id`
  - Get specific group details by ID
  - **Use Case**: Retrieve group information using ID

#### Group Members (Server)
- ❌ **Get group members (Server)** - `GET /rest/api/group/{groupName}/member`
  - Get members for Confluence Server (different endpoint than Cloud)
  - **Use Case**: List group members on Server installations

### 5. Attachment Domain - Missing Features

#### Attachment Versions
- ❌ **Get attachment version** - `GET /rest/api/content/{id}/version/{versionNumber}`
  - Get specific version of an attachment
  - **Use Case**: Access previous versions of files

### 6. Missing Domains

#### Audit Domain
**Reference**: [Confluence Server REST API - Audit](https://docs.atlassian.com/atlassian-confluence/REST/latest/#audit)

- ❌ **Get audit records** - `GET /rest/api/audit`
  - Retrieve audit log entries
  - **Use Case**: Track user actions and changes
  
- ❌ **Get audit record** - `GET /rest/api/audit/{number}`
  - Get specific audit record
  - **Use Case**: View details of specific audit event
  
- ❌ **Create audit record** - `POST /rest/api/audit`
  - Create custom audit record
  - **Use Case**: Log custom events
  
- ❌ **Export audit records** - `GET /rest/api/audit/export`
  - Export audit log
  - **Use Case**: Archive audit data

#### Longtask Domain
**Reference**: [Confluence REST API - Longtask](https://developer.atlassian.com/cloud/confluence/rest/v1/api-group-longtask/)

- ❌ **Get long-running tasks** - `GET /rest/api/longtask`
  - Get all long-running tasks
  - **Use Case**: Monitor background operations
  
- ❌ **Get long-running task** - `GET /rest/api/longtask/{id}`
  - Get status of specific long-running task
  - **Use Case**: Check progress of space deletion, etc.

#### Search Domain (Enhanced)
**Reference**: [Confluence Cloud REST API - Search](https://developer.atlassian.com/cloud/confluence/rest/v1/api-group-search/)

- ❌ **Search by CQL (enhanced)** - `GET /rest/api/search`
  - Enhanced search with excerpt highlighting
  - **Use Case**: Full-text search with context
  
*Note: Basic CQL search is implemented via Content.SearchAsync*

#### Content Blueprint Domain
**Reference**: [Confluence REST API - Blueprint](https://docs.atlassian.com/atlassian-confluence/REST/latest/#content-blueprint)

- ❌ **Get blueprints** - `GET /rest/api/space/{spaceKey}/content-blueprint`
  - Get available blueprints for a space
  - **Use Case**: List page creation templates
  
- ❌ **Create content from blueprint** - `POST /rest/api/content-blueprint`
  - Create content using a blueprint
  - **Use Case**: Create standardized pages

#### Relation Domain
**Reference**: [Confluence REST API - Relation](https://docs.atlassian.com/atlassian-confluence/REST/latest/#relation)

- ❌ **Get relation** - `GET /rest/api/relation/{relationName}/from/{sourceType}/{sourceKey}/to/{targetType}/{targetKey}`
  - Get relationships between content
  - **Use Case**: Find linked pages
  
- ❌ **Create relation** - `PUT /rest/api/relation/{relationName}/from/{sourceType}/{sourceKey}/to/{targetType}/{targetKey}`
  - Create relationship between content
  - **Use Case**: Link related pages

#### Settings Domain
**Reference**: [Confluence REST API - Settings](https://developer.atlassian.com/cloud/confluence/rest/v1/api-group-settings/)

- ❌ **Get look and feel settings** - `GET /rest/api/settings/lookandfeel`
  - Get global look and feel settings
  - **Use Case**: Retrieve theme configuration
  
- ❌ **Update look and feel settings** - `PUT /rest/api/settings/lookandfeel`
  - Update global look and feel
  - **Use Case**: Apply global styling
  
- ❌ **Reset look and feel settings** - `DELETE /rest/api/settings/lookandfeel`
  - Reset to default look and feel
  - **Use Case**: Remove custom global styling

*Note: System info is partially implemented via Misc.GetSystemInfoAsync*

#### Analytics Domain (Cloud Only)
**Reference**: [Confluence Cloud Analytics API](https://developer.atlassian.com/cloud/confluence/rest/v1/api-group-analytics/)

- ❌ **Get content views** - `GET /rest/api/analytics/content/{contentId}/viewers`
  - Get view statistics for content
  - **Use Case**: Track page popularity
  
- ❌ **Get popular content** - `GET /rest/api/analytics/content/popular`
  - Get most viewed content
  - **Use Case**: Identify trending pages

### 7. Confluence Data Center / Server Specific

#### Cluster Domain
**Reference**: [Confluence Data Center REST API - Cluster](https://docs.atlassian.com/atlassian-confluence/REST/latest/#cluster)

- ❌ **Get cluster information** - `GET /rest/api/cluster`
  - Get cluster node information
  - **Use Case**: Monitor Data Center cluster health
  
- ❌ **Get cluster node** - `GET /rest/api/cluster/{clusterNodeId}`
  - Get specific cluster node details
  - **Use Case**: Check individual node status

### 8. Macro and Rendering

#### Macro Domain
- ❌ **Get macro body** - Related to content conversion
  - Process and render macros
  - **Use Case**: Preview macro output

### 9. Inline Tasks

**Reference**: [Confluence Cloud REST API - Inline Tasks](https://developer.atlassian.com/cloud/confluence/rest/v2/api-group-inline-tasks/)

- ❌ **Get inline tasks** - `GET /api/v2/inline-tasks`
  - Get inline tasks from content
  - **Use Case**: Extract and manage tasks from pages
  
- ❌ **Update inline task** - `PUT /api/v2/inline-tasks/{inlineTaskId}`
  - Update task status
  - **Use Case**: Mark tasks as complete

### 10. Custom Content

**Reference**: [Confluence Cloud REST API - Custom Content](https://developer.atlassian.com/cloud/confluence/rest/v2/api-group-custom-content/)

- ❌ **Create custom content** - `POST /api/v2/custom-content`
  - Create custom content types
  - **Use Case**: Store structured data in Confluence
  
- ❌ **Get custom content** - `GET /api/v2/custom-content/{id}`
  - Retrieve custom content
  - **Use Case**: Access custom data structures

## Obsolete or Deprecated Features

### 1. Removed in Confluence Cloud

#### User Keys (Deprecated)
- ⚠️ **User.UserKey property** may be deprecated in Cloud
  - Cloud uses `accountId` exclusively
  - Server uses `username`
  - **Current Status**: Library handles both via `IUserIdentifier`
  - **Recommendation**: Continue supporting both for backward compatibility

### 2. Confluence Server API Deprecations

#### Personal Spaces
- ⚠️ **Personal Spaces** - Being phased out in newer versions
  - `ContentTypes.Personal` enum exists but functionality limited
  - **Reference**: [Atlassian announcement on personal spaces](https://confluence.atlassian.com/doc/confluence-7-0-release-notes-990546530.html)
  
#### Anonymous Access
- ⚠️ **Anonymous user operations** may have limited support
  - `GetAnonymousUserAsync()` implemented but Cloud restricts anonymous access
  - **Current Status**: Works on Server, limited on Cloud

### 3. API Version Considerations

#### REST API v1 vs v2
- ⚠️ **Library uses REST API v1** exclusively
  - Atlassian is developing v2 API with different structure
  - **Reference**: [Confluence Cloud REST API v2](https://developer.atlassian.com/cloud/confluence/rest/v2/intro/)
  - **Impact**: New v2 endpoints not accessible
  - **Examples**: 
    - Inline tasks (v2 only)
    - Custom content (v2 only)
    - Enhanced page operations (v2 improvements)

### 4. Attachment Deletion Issues

#### Attachment Delete Workaround
- ⚠️ **DeleteAsync(Content attachment)** uses workaround endpoint
  - References [CONF-36015](https://jira.atlassian.com/browse/CONF-36015)
  - Uses `json/removeattachmentversion.action` instead of REST API
  - **Status**: May break if Atlassian changes this endpoint
  - **File**: [src/Dapplo.Confluence/AttachmentExtensions.cs](src/Dapplo.Confluence/AttachmentExtensions.cs#L70-92)

### 5. PDF Export via FlyingPDF

#### FlyingPDF Deprecation
- ⚠️ **GetPdfAsync** uses FlyingPDF export
  - FlyingPDF is legacy export mechanism
  - **Status**: Still works but may be deprecated in future
  - **Alternative**: Native PDF export API (not yet implemented)
  - **File**: [src/Dapplo.Confluence/ContentExtensions.cs](src/Dapplo.Confluence/ContentExtensions.cs#L457-482)

### 6. Missing Content Field Expansions

#### Incomplete Expand Support
- ⚠️ **Limited expand parameters** in some operations
  - Not all expand options are utilized
  - Missing expansions for:
    - `restrictions` - content permissions
    - `operations` - available operations on content
    - `schedulePublishDate` - scheduled publishing
    - `schedulePublishInfo` - publish scheduling details
    - `children.attachment` - inline attachment expansion
    - `children.comment` - inline comment expansion
    - `macroRenderedOutput` - macro rendering
  - **Reference**: [Expansion documentation](https://developer.atlassian.com/cloud/confluence/rest/v1/intro/#expansion)

## Feature Requests

The following sections provide actionable feature requests formatted for GitHub Copilot or manual implementation.

### High Priority Feature Requests

#### FR-001: Content Restrictions Management
**Title**: Add support for content restrictions (permissions) management

**Description**: Implement methods to get, add, update, and delete content restrictions to control who can view or edit specific pages.

**API Endpoints**:
- `GET /rest/api/content/{id}/restriction`
- `POST /rest/api/content/{id}/restriction`
- `PUT /rest/api/content/{id}/restriction`
- `DELETE /rest/api/content/{id}/restriction/byGroup/{groupName}`

**Implementation**:
- Create `IContentRestrictionDomain` interface
- Add extension methods in `ContentRestrictionExtensions.cs`
- Create entity classes: `ContentRestriction`, `RestrictionUser`, `RestrictionGroup`
- Support both user-based and group-based restrictions

**Files to Create/Modify**:
- Create: `src/Dapplo.Confluence/ContentRestrictionExtensions.cs`
- Create: `src/Dapplo.Confluence/Entities/ContentRestriction.cs`
- Modify: `src/Dapplo.Confluence/IConfluenceClient.cs` (add domain)

**Reference**: https://developer.atlassian.com/cloud/confluence/rest/v1/api-group-content-restrictions/

---

#### FR-002: Content Properties Support
**Title**: Add support for content properties (custom metadata)

**Description**: Implement methods to manage custom properties attached to content for storing metadata and custom application data.

**API Endpoints**:
- `GET /rest/api/content/{id}/property`
- `GET /rest/api/content/{id}/property/{key}`
- `POST /rest/api/content/{id}/property`
- `PUT /rest/api/content/{id}/property/{key}`
- `DELETE /rest/api/content/{id}/property/{key}`

**Implementation**:
- Add extension methods to `ContentExtensions.cs` or create separate `ContentPropertyExtensions.cs`
- Create entity class: `ContentProperty`
- Support generic property values (JSON serialization)

**Files to Create/Modify**:
- Modify: `src/Dapplo.Confluence/ContentExtensions.cs`
- Create: `src/Dapplo.Confluence/Entities/ContentProperty.cs`

**Reference**: https://developer.atlassian.com/cloud/confluence/rest/v1/api-group-content-properties/

---

#### FR-003: Space Permissions Management
**Title**: Add support for space permissions management

**Description**: Implement methods to get, add, and remove space-level permissions for users and groups.

**API Endpoints**:
- `GET /rest/api/space/{spaceKey}/permission`
- `POST /rest/api/space/{spaceKey}/permission`
- `DELETE /rest/api/space/{spaceKey}/permission/{id}`

**Implementation**:
- Add extension methods to `SpaceExtensions.cs`
- Create entity classes: `SpacePermission`, `SpacePermissionSubject`
- Support different permission types (read, write, admin, etc.)

**Files to Create/Modify**:
- Modify: `src/Dapplo.Confluence/SpaceExtensions.cs`
- Create: `src/Dapplo.Confluence/Entities/SpacePermission.cs`

**Reference**: https://developer.atlassian.com/cloud/confluence/rest/v1/api-group-space-permissions/

---

#### FR-004: Comment Management
**Title**: Add support for creating, updating, and managing comments

**Description**: Implement full comment lifecycle management including retrieval, creation, updating, and deletion of page comments.

**API Endpoints**:
- `GET /rest/api/content/{id}/child/comment`
- `POST /rest/api/content` (with type=comment)
- `PUT /rest/api/content/{id}` (for comments)
- `DELETE /rest/api/content/{id}` (for comments)

**Implementation**:
- Add extension methods to `ContentExtensions.cs` or create `CommentExtensions.cs`
- Utilize existing `Content` entity with `ContentTypes.Comment`
- Add comment-specific helper methods

**Files to Create/Modify**:
- Modify: `src/Dapplo.Confluence/ContentExtensions.cs`
- May need: `src/Dapplo.Confluence/Entities/Comment.cs` (or extend Content)

**Reference**: https://developer.atlassian.com/cloud/confluence/rest/v1/api-group-content/

---

#### FR-005: Content Version History
**Title**: Add support for content version management

**Description**: Implement methods to get version history, retrieve specific versions, and restore previous versions of content.

**API Endpoints**:
- `GET /rest/api/content/{id}/version`
- `GET /rest/api/content/{id}/version/{versionNumber}`
- `POST /rest/api/content/{id}/version` (restore)

**Implementation**:
- Add extension methods to `ContentExtensions.cs`
- Extend existing `Version` and `History` entities as needed
- Add restore functionality

**Files to Create/Modify**:
- Modify: `src/Dapplo.Confluence/ContentExtensions.cs`
- Review: `src/Dapplo.Confluence/Entities/Version.cs`
- Review: `src/Dapplo.Confluence/Entities/History.cs`

**Reference**: https://developer.atlassian.com/cloud/confluence/rest/v1/api-group-content-versions/

---

### Medium Priority Feature Requests

#### FR-006: User Search
**Title**: Add user search functionality

**Description**: Implement user search by name, email, or other criteria.

**API Endpoints**:
- `GET /rest/api/user/search?query={query}`
- `GET /rest/api/search/user?cql={cql}`

**Implementation**:
- Add extension methods to `UserExtensions.cs`
- Support both simple query and CQL-based search
- Handle Cloud vs Server differences

**Files to Create/Modify**:
- Modify: `src/Dapplo.Confluence/UserExtensions.cs`

**Reference**: https://developer.atlassian.com/cloud/confluence/rest/v1/api-group-users/

---

#### FR-007: Descendants Support
**Title**: Add support for getting all descendants of content

**Description**: Implement methods to retrieve all descendant content (not just immediate children) for a given page.

**API Endpoints**:
- `GET /rest/api/content/{id}/descendant`
- `GET /rest/api/content/{id}/descendant/{type}`

**Implementation**:
- Add extension methods to `ContentExtensions.cs`
- Create result type for descendant tree
- Support filtering by content type

**Files to Create/Modify**:
- Modify: `src/Dapplo.Confluence/ContentExtensions.cs`

**Reference**: https://developer.atlassian.com/cloud/confluence/rest/v1/api-group-content-descendants/

---

#### FR-008: Content Templates
**Title**: Add content template management

**Description**: Implement methods to get, create, update, and delete content templates.

**API Endpoints**:
- `GET /rest/api/template`
- `GET /rest/api/template/{contentTemplateId}`
- `POST /rest/api/template`
- `PUT /rest/api/template`
- `DELETE /rest/api/template/{contentTemplateId}`

**Implementation**:
- Create new domain: `ITemplateDomain`
- Create `TemplateExtensions.cs`
- Create entity class: `ContentTemplate`

**Files to Create/Modify**:
- Create: `src/Dapplo.Confluence/TemplateExtensions.cs`
- Create: `src/Dapplo.Confluence/Entities/ContentTemplate.cs`
- Modify: `src/Dapplo.Confluence/IConfluenceClient.cs`

**Reference**: https://developer.atlassian.com/cloud/confluence/rest/v1/api-group-content-templates/

---

#### FR-009: Space Properties
**Title**: Add space properties (custom metadata) support

**Description**: Implement methods to manage custom properties attached to spaces.

**API Endpoints**:
- `GET /rest/api/space/{spaceKey}/property`
- `POST /rest/api/space/{spaceKey}/property`
- `PUT /rest/api/space/{spaceKey}/property/{key}`
- `DELETE /rest/api/space/{spaceKey}/property/{key}`

**Implementation**:
- Add extension methods to `SpaceExtensions.cs`
- Create entity class: `SpaceProperty`

**Files to Create/Modify**:
- Modify: `src/Dapplo.Confluence/SpaceExtensions.cs`
- Create: `src/Dapplo.Confluence/Entities/SpaceProperty.cs`

**Reference**: https://developer.atlassian.com/cloud/confluence/rest/v1/api-group-space-properties/

---

#### FR-010: Audit Log Access
**Title**: Add audit log retrieval capabilities

**Description**: Implement methods to access and export audit logs (Server/Data Center feature).

**API Endpoints**:
- `GET /rest/api/audit`
- `GET /rest/api/audit/{number}`
- `GET /rest/api/audit/export`

**Implementation**:
- Create new domain: `IAuditDomain`
- Create `AuditExtensions.cs`
- Create entity classes: `AuditRecord`, `AuditDetails`
- Note: Server/Data Center only

**Files to Create/Modify**:
- Create: `src/Dapplo.Confluence/AuditExtensions.cs`
- Create: `src/Dapplo.Confluence/Entities/AuditRecord.cs`
- Modify: `src/Dapplo.Confluence/IConfluenceClient.cs`

**Reference**: https://docs.atlassian.com/atlassian-confluence/REST/latest/#audit

---

### Low Priority Feature Requests

#### FR-011: Long-running Task Monitoring
**Title**: Add better support for monitoring long-running tasks

**Description**: Enhance the existing `LongRunningTask` support with status checking and progress monitoring.

**API Endpoints**:
- `GET /rest/api/longtask`
- `GET /rest/api/longtask/{id}`

**Implementation**:
- Create new domain: `ILongTaskDomain`
- Create `LongTaskExtensions.cs`
- Extend existing `LongRunningTask` entity

**Files to Create/Modify**:
- Create: `src/Dapplo.Confluence/LongTaskExtensions.cs`
- Review: `src/Dapplo.Confluence/Entities/LongRunningTask.cs`
- Modify: `src/Dapplo.Confluence/IConfluenceClient.cs`

**Reference**: https://developer.atlassian.com/cloud/confluence/rest/v1/api-group-longtask/

---

#### FR-012: Content Body Conversion
**Title**: Add content body format conversion

**Description**: Implement methods to convert content between different formats (storage, view, export_view, etc.).

**API Endpoints**:
- `POST /rest/api/contentbody/convert/{to}`

**Implementation**:
- Add extension methods to `ContentExtensions.cs` or `MiscExtensions.cs`
- Support conversion between all format types
- Create conversion request/response entities

**Files to Create/Modify**:
- Modify: `src/Dapplo.Confluence/ContentExtensions.cs` or `MiscExtensions.cs`

**Reference**: https://developer.atlassian.com/cloud/confluence/rest/v1/api-group-content-body/

---

#### FR-013: Space Settings Management
**Title**: Add space settings configuration

**Description**: Implement methods to get and update space settings and preferences.

**API Endpoints**:
- `GET /rest/api/space/{spaceKey}/settings`
- `PUT /rest/api/space/{spaceKey}/settings`

**Implementation**:
- Add extension methods to `SpaceExtensions.cs`
- Create entity class: `SpaceSettings`

**Files to Create/Modify**:
- Modify: `src/Dapplo.Confluence/SpaceExtensions.cs`
- Create: `src/Dapplo.Confluence/Entities/SpaceSettings.cs`

**Reference**: https://developer.atlassian.com/cloud/confluence/rest/v1/api-group-space-settings/

---

#### FR-014: REST API v2 Support
**Title**: Add support for Confluence Cloud REST API v2

**Description**: Implement v2 API endpoints alongside existing v1 endpoints, particularly for new features only available in v2.

**API Endpoints**: Various v2 endpoints
- `/api/v2/pages`
- `/api/v2/blogposts`
- `/api/v2/inline-tasks`
- `/api/v2/custom-content`

**Implementation**:
- Create parallel v2 extension methods
- Add configuration to choose API version
- Maintain backward compatibility with v1

**Files to Create/Modify**:
- Multiple new files for v2 implementations
- Modify: `src/Dapplo.Confluence/ConfluenceClientConfig.cs`

**Reference**: https://developer.atlassian.com/cloud/confluence/rest/v2/intro/

---

#### FR-015: Enhanced Expand Parameters
**Title**: Add support for additional expand parameters

**Description**: Implement support for all available expand parameters in content and space operations.

**Missing Expansions**:
- `restrictions` - content permissions
- `operations` - available operations
- `schedulePublishDate` - scheduled publishing
- `children.attachment` - inline attachment expansion
- `children.comment` - inline comment expansion
- `macroRenderedOutput` - macro rendering

**Implementation**:
- Update `ConfluenceClientConfig` with new expand defaults
- Ensure all Get operations respect new expand options
- Add strongly-typed expand parameter builder

**Files to Create/Modify**:
- Modify: `src/Dapplo.Confluence/ConfluenceClientConfig.cs`
- Review all extension methods using expand parameters

**Reference**: https://developer.atlassian.com/cloud/confluence/rest/v1/intro/#expansion

---

## Migration and Deprecation Warnings

### For Users Migrating from Server to Cloud

1. **User Identification**: Update code to use `accountId` instead of `username` when targeting Cloud
2. **Anonymous Access**: Review usage of `GetAnonymousUserAsync()` - limited on Cloud
3. **Label Watching**: Note that `AddLabelWatcher()` and related methods are Cloud-only
4. **Content Move/Copy**: These operations are Cloud-only
5. **Personal Spaces**: Avoid creating new personal spaces; they're deprecated

### Future-Proofing Recommendations

1. **PDF Export**: Consider migrating from FlyingPDF to native export when available
2. **Attachment Deletion**: Monitor [CONF-36015](https://jira.atlassian.com/browse/CONF-36015) for official REST API support
3. **API Version**: Prepare for REST API v2 migration for Cloud deployments
4. **Expand Parameters**: Start using newer expand options as they become available

## Contributing

When implementing new features from this list:

1. **Reference**: Always link to the official Atlassian API documentation
2. **Tests**: Add integration tests in the appropriate test file
3. **Cloud vs Server**: Handle differences between Confluence Cloud and Server where applicable
4. **Entities**: Create or extend entities in `src/Dapplo.Confluence/Entities/`
5. **Extensions**: Follow the existing pattern of domain-specific extension methods
6. **Documentation**: Update this document as features are implemented

## Useful Links

- [Confluence Cloud REST API v1](https://developer.atlassian.com/cloud/confluence/rest/v1/intro/)
- [Confluence Cloud REST API v2](https://developer.atlassian.com/cloud/confluence/rest/v2/intro/)
- [Confluence Server REST API](https://docs.atlassian.com/atlassian-confluence/REST/latest/)
- [Confluence Query Language (CQL)](https://developer.atlassian.com/cloud/confluence/advanced-searching-using-cql/)
- [Confluence API Expansion](https://developer.atlassian.com/cloud/confluence/rest/v1/intro/#expansion)

---

**Document Version**: 1.0  
**Last Updated**: 2026-01-30  
**Maintained By**: Dapplo.Confluence Contributors
