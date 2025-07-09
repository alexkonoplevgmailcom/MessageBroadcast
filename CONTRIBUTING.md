# Contributing to MessageBroadcast

Thank you for your interest in contributing to MessageBroadcast! This guide will help you get started with contributing to this high-performance message broadcasting system.

## 🎯 Project Overview

MessageBroadcast is a production-ready, high-performance message broadcasting system built with .NET 9 and SignalR. Our goal is to maintain a clean, efficient, and well-documented codebase that serves as a reference implementation for real-time messaging systems.

## 🚀 Quick Start for Contributors

### Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) (latest stable version)
- [Git](https://git-scm.com/) for version control
- IDE: [Visual Studio 2022](https://visualstudio.microsoft.com/), [VS Code](https://code.visualstudio.com/), or [JetBrains Rider](https://www.jetbrains.com/rider/)

### Setup Development Environment

1. **Fork and Clone**
   ```bash
   # Fork the repository on GitHub, then clone your fork
   git clone https://github.com/YOUR-USERNAME/MessageBroadcast.git
   cd MessageBroadcast
   ```

2. **Restore Dependencies**
   ```bash
   dotnet restore
   ```

3. **Build Solution**
   ```bash
   dotnet build
   ```

4. **Run Tests**
   ```bash
   dotnet test
   ```

5. **Start Development Server**
   ```bash
   cd src/MessageBroadcast.Server
   dotnet run
   ```

## 📋 How to Contribute

### Types of Contributions Welcome

- 🐛 **Bug Reports**: Issues with existing functionality
- ✨ **Feature Requests**: New capabilities that align with project goals
- 📚 **Documentation**: Improvements to docs, examples, or code comments
- 🧪 **Testing**: Additional test cases or test improvements
- ⚡ **Performance**: Optimizations and performance improvements
- 🔧 **Tooling**: Development workflow and automation improvements

### Contribution Process

1. **Check Existing Issues**
   - Search [existing issues](https://github.com/your-username/MessageBroadcast/issues) before creating new ones
   - Look for issues labeled `good first issue` for newcomers

2. **Create or Comment on Issue**
   - For bugs: Provide reproduction steps, environment details
   - For features: Describe use case, expected behavior, and benefits
   - For large changes: Discuss approach before implementing

3. **Create Feature Branch**
   ```bash
   git checkout -b feature/your-feature-name
   # or
   git checkout -b bugfix/issue-description
   ```

4. **Make Changes**
   - Follow our [coding standards](#-coding-standards)
   - Add/update tests for new functionality
   - Update documentation as needed

5. **Test Your Changes**
   ```bash
   # Run all tests
   dotnet test
   
   # Run load tests (if applicable)
   cd tests/MessageBroadcast.LoadTest
   dotnet run http://localhost:5001 10 20 100 30
   ```

6. **Commit and Push**
   ```bash
   git add .
   git commit -m "feat: add new broadcasting feature"
   git push origin feature/your-feature-name
   ```

7. **Create Pull Request**
   - Use descriptive title and description
   - Reference any related issues
   - Include screenshots for UI changes
   - Ensure all checks pass

## 🎨 Coding Standards

### Code Style

- **Language**: C# 12 with latest language features
- **Async**: Use `async`/`await` for all I/O operations
- **Naming**: PascalCase for public members, camelCase for private
- **Documentation**: XML documentation for public APIs

### Code Organization

```
src/
├── MessageBroadcast.Server/     # Server application
├── MessageBroadcast.Client/     # Client library
└── MessageBroadcast.Shared/     # Shared contracts

tests/
├── MessageBroadcast.Server.Tests/  # Server unit tests
├── MessageBroadcast.Client.Tests/  # Client unit tests
└── MessageBroadcast.LoadTest/       # Performance tests
```

### Performance Guidelines

- **Async Operations**: All I/O should be async
- **Memory Allocation**: Minimize allocations in hot paths
- **Exception Handling**: Use appropriate exception types
- **Resource Disposal**: Implement `IDisposable`/`IAsyncDisposable` properly

### Example Code Style

```csharp
// ✅ Good: Async, descriptive naming, proper disposal
public async Task<bool> PublishMessageAsync(
    string content, 
    string senderId, 
    CancellationToken cancellationToken = default)
{
    ArgumentException.ThrowIfNullOrEmpty(content);
    ArgumentException.ThrowIfNullOrEmpty(senderId);

    try
    {
        var message = new BroadcastMessage
        {
            Content = content,
            SenderId = senderId,
            Timestamp = DateTime.UtcNow,
            MessageId = Guid.NewGuid().ToString()
        };

        await _connection.InvokeAsync("SendMessage", message, cancellationToken);
        return true;
    }
    catch (Exception ex) when (ex is not OperationCanceledException)
    {
        // Log error (implementation specific)
        return false;
    }
}
```

## 🧪 Testing Guidelines

### Test Structure
- **Unit Tests**: Focus on individual component behavior
- **Integration Tests**: Test component interactions
- **Load Tests**: Validate performance characteristics

### Writing Tests

```csharp
[Fact]
public async Task PublishMessageAsync_WithValidMessage_ShouldReturnTrue()
{
    // Arrange
    var client = new MessageBroadcastClient("http://localhost:5001");
    await client.ConnectAsync();

    // Act
    var result = await client.PublishMessageAsync("Test message", "TestPublisher");

    // Assert
    Assert.True(result);
}
```

### Test Categories
- Use `[Fact]` for simple tests
- Use `[Theory]` with `[InlineData]` for parameterized tests
- Use `[Trait("Category", "Integration")]` for integration tests

## 📚 Documentation Standards

### Code Documentation
- All public APIs must have XML documentation
- Include parameter descriptions and return value information
- Provide usage examples for complex APIs

### README Updates
- Update performance metrics if changes affect performance
- Add new features to the feature list
- Update code examples if APIs change

### Documentation Files
- `README.md`: Main project documentation
- `docs/api-reference.md`: Complete API documentation
- `docs/architecture.md`: System design and architecture
- `docs/deployment.md`: Deployment and configuration guide

## 🐛 Bug Reports

### Information to Include

1. **Environment Details**
   - .NET version
   - Operating system and version
   - MessageBroadcast version

2. **Reproduction Steps**
   - Minimal code example
   - Expected vs actual behavior
   - Error messages and stack traces

3. **Additional Context**
   - Performance impact
   - Workarounds attempted
   - Related issues

### Bug Report Template

```markdown
**Environment:**
- .NET Version: 9.0
- OS: Windows 11 / macOS 14 / Ubuntu 22.04
- MessageBroadcast Version: 1.0.0

**Description:**
Brief description of the issue

**Reproduction Steps:**
1. Start server with default configuration
2. Connect client with...
3. Send message with...
4. Observe error...

**Expected Behavior:**
What should have happened

**Actual Behavior:**
What actually happened

**Additional Context:**
Any other relevant information
```

## ✨ Feature Requests

### Guidelines for Feature Requests

- **Align with Project Goals**: Features should support high-performance messaging
- **Maintain Simplicity**: Avoid features that add unnecessary complexity
- **Consider Performance**: New features shouldn't degrade performance
- **Documentation**: Include examples of how the feature would be used

### Feature Request Template

```markdown
**Feature Description:**
Clear description of the proposed feature

**Use Case:**
Specific scenario where this feature would be beneficial

**Proposed API:**
Example of how the feature would be used

**Alternatives Considered:**
Other approaches that were considered

**Additional Context:**
Performance implications, breaking changes, etc.
```

## 🏷️ Commit Message Format

Use conventional commit format for clear history:

```
type(scope): description

[optional body]

[optional footer]
```

### Types
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `perf`: Performance improvements
- `test`: Adding or updating tests
- `refactor`: Code refactoring
- `style`: Code style changes
- `ci`: CI/CD changes

### Examples
```
feat(client): add automatic reconnection with exponential backoff

fix(server): resolve memory leak in connection manager

docs(api): update examples in API reference

perf(broadcast): optimize message serialization for 20% improvement
```

## 🔍 Code Review Process

### Review Criteria
- **Functionality**: Does the code work as intended?
- **Performance**: Are there performance implications?
- **Testing**: Are changes adequately tested?
- **Documentation**: Is documentation updated appropriately?
- **Style**: Does code follow project conventions?

### Review Checklist
- [ ] Code builds without warnings
- [ ] All tests pass
- [ ] New functionality has tests
- [ ] Public APIs have documentation
- [ ] Performance impact considered
- [ ] No breaking changes (or properly documented)

## 🚀 Release Process

### Version Numbers
We follow [Semantic Versioning](https://semver.org/):
- **MAJOR**: Breaking changes
- **MINOR**: New features (backward compatible)
- **PATCH**: Bug fixes

### Release Checklist
- [ ] All tests pass
- [ ] Performance benchmarks updated
- [ ] Documentation updated
- [ ] CHANGELOG.md updated
- [ ] Version numbers bumped
- [ ] Release notes prepared

## 📞 Getting Help

### Communication Channels
- **Issues**: For bugs and feature requests
- **Discussions**: For questions and general discussion
- **Email**: For security-related issues

### Response Times
- **Bug Reports**: Within 2-3 business days
- **Feature Requests**: Within 1 week
- **Pull Requests**: Within 1 week

## 📄 License

By contributing to MessageBroadcast, you agree that your contributions will be licensed under the [MIT License](LICENSE).

## 🙏 Recognition

Contributors will be recognized in:
- Project README.md
- Release notes for significant contributions
- GitHub contributors page

---

Thank you for contributing to MessageBroadcast! Your help makes this project better for everyone. 🚀
