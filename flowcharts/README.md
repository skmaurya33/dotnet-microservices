# Flowcharts Directory

This directory contains flowchart diagrams for the microservices architecture, specifically showing the JWT token flow between services.

## Files

- `jwt-token-flow.md` - Complete documentation with the Mermaid diagram
- `jwt-token-flow.mmd` - Pure Mermaid diagram file
- `README.md` - This file

## How to Use

### 1. View in Markdown Editors

Open `jwt-token-flow.md` in any Markdown editor that supports Mermaid diagrams:
- VS Code (with Mermaid extension)
- Typora
- GitHub (when committed to repository)
- GitLab
- Notion

### 2. Convert to Images

#### Using Mermaid CLI

Install Mermaid CLI globally:
```bash
npm install -g @mermaid-js/mermaid-cli
```

Convert to PNG:
```bash
mmdc -i jwt-token-flow.mmd -o jwt-token-flow.png
```

Convert to SVG:
```bash
mmdc -i jwt-token-flow.mmd -o jwt-token-flow.svg
```

Convert to PDF:
```bash
mmdc -i jwt-token-flow.mmd -o jwt-token-flow.pdf
```

#### Using Online Tools

1. **Mermaid Live Editor**: https://mermaid.live/
   - Copy the content from `jwt-token-flow.mmd`
   - Paste it in the editor
   - Download as PNG, SVG, or PDF

2. **Draw.io**: https://app.diagrams.net/
   - Import the Mermaid file
   - Export in various formats

### 3. Integration Options

#### In Documentation
- Copy the Mermaid code block into your documentation
- Most modern documentation platforms support Mermaid

#### In Presentations
- Convert to image format and insert into PowerPoint/Google Slides
- Use SVG for scalable graphics

#### In Code Comments
- Reference the diagram file location
- Include the raw Mermaid code in comments

## Diagram Description

The JWT Token Flow diagram shows:

1. **Client Request**: Client sends request with JWT token in Authorization header
2. **Token Extraction**: Blog Controller extracts the token from the header
3. **Token Passing**: Token is passed to UserService methods
4. **Service Communication**: UserService uses token to authenticate with Auth Service
5. **Data Retrieval**: Auth Service validates token and returns user data
6. **Response**: Data flows back through the chain to the client

## Benefits of This Approach

- **Efficiency**: Reuses existing JWT tokens instead of generating new ones
- **Security**: Maintains authentication throughout the service chain
- **Simplicity**: Eliminates complex service-to-service authentication
- **Performance**: Reduces token generation overhead
- **Consistency**: Uses same token for entire request lifecycle

## Maintenance

When updating the architecture:
1. Update the `.mmd` file with new diagram code
2. Update the `.md` file with new documentation
3. Regenerate image files if needed
4. Update this README if new files are added 