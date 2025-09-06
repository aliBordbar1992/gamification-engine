using GamificationEngine.Application.Abstractions;
using GamificationEngine.Application.DTOs;
using GamificationEngine.Application.Services;
using GamificationEngine.Domain.Entities;
using GamificationEngine.Domain.Repositories;
using Moq;
using Shouldly;

namespace GamificationEngine.Application.Tests.Services;

/// <summary>
/// Unit tests for EntityManagementService
/// </summary>
public class EntityManagementServiceTests
{
    private readonly Mock<IPointCategoryRepository> _mockPointCategoryRepository;
    private readonly Mock<IBadgeRepository> _mockBadgeRepository;
    private readonly Mock<ITrophyRepository> _mockTrophyRepository;
    private readonly Mock<ILevelRepository> _mockLevelRepository;
    private readonly EntityManagementService _service;

    public EntityManagementServiceTests()
    {
        _mockPointCategoryRepository = new Mock<IPointCategoryRepository>();
        _mockBadgeRepository = new Mock<IBadgeRepository>();
        _mockTrophyRepository = new Mock<ITrophyRepository>();
        _mockLevelRepository = new Mock<ILevelRepository>();

        _service = new EntityManagementService(
            _mockPointCategoryRepository.Object,
            _mockBadgeRepository.Object,
            _mockTrophyRepository.Object,
            _mockLevelRepository.Object);
    }

    #region Point Categories Tests

    [Fact]
    public async Task GetAllPointCategoriesAsync_ShouldReturnAllCategories()
    {
        // Arrange
        var categories = new List<PointCategory>
        {
            new("xp", "Experience", "XP points", "sum"),
            new("score", "Score", "Score points", "max")
        };
        _mockPointCategoryRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        // Act
        var result = await _service.GetAllPointCategoriesAsync();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Count().ShouldBe(2);
        result.Value.ShouldContain(pc => pc.Id == "xp");
        result.Value.ShouldContain(pc => pc.Id == "score");
    }

    [Fact]
    public async Task GetPointCategoryByIdAsync_WithValidId_ShouldReturnCategory()
    {
        // Arrange
        var category = new PointCategory("xp", "Experience", "XP points", "sum");
        _mockPointCategoryRepository.Setup(x => x.GetByIdAsync("xp", It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Act
        var result = await _service.GetPointCategoryByIdAsync("xp");

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Id.ShouldBe("xp");
    }

    [Fact]
    public async Task GetPointCategoryByIdAsync_WithEmptyId_ShouldReturnFailure()
    {
        // Act
        var result = await _service.GetPointCategoryByIdAsync("");

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldContain("ID cannot be empty");
    }

    [Fact]
    public async Task GetPointCategoryByIdAsync_WithNonExistentId_ShouldReturnFailure()
    {
        // Arrange
        _mockPointCategoryRepository.Setup(x => x.GetByIdAsync("nonexistent", It.IsAny<CancellationToken>()))
            .ReturnsAsync((PointCategory?)null);

        // Act
        var result = await _service.GetPointCategoryByIdAsync("nonexistent");

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldContain("not found");
    }

    [Fact]
    public async Task CreatePointCategoryAsync_WithValidData_ShouldCreateCategory()
    {
        // Arrange
        var dto = new CreatePointCategoryDto
        {
            Id = "xp",
            Name = "Experience",
            Description = "XP points",
            Aggregation = "sum"
        };
        _mockPointCategoryRepository.Setup(x => x.ExistsAsync("xp", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _service.CreatePointCategoryAsync(dto);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Id.ShouldBe("xp");
        _mockPointCategoryRepository.Verify(x => x.AddAsync(It.IsAny<PointCategory>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreatePointCategoryAsync_WithExistingId_ShouldReturnFailure()
    {
        // Arrange
        var dto = new CreatePointCategoryDto
        {
            Id = "xp",
            Name = "Experience",
            Description = "XP points",
            Aggregation = "sum"
        };
        _mockPointCategoryRepository.Setup(x => x.ExistsAsync("xp", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.CreatePointCategoryAsync(dto);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldContain("already exists");
    }

    [Fact]
    public async Task CreatePointCategoryAsync_WithNullDto_ShouldReturnFailure()
    {
        // Act
        var result = await _service.CreatePointCategoryAsync(null!);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldContain("DTO cannot be null");
    }

    [Fact]
    public async Task UpdatePointCategoryAsync_WithValidData_ShouldUpdateCategory()
    {
        // Arrange
        var category = new PointCategory("xp", "Old Name", "Old description", "sum");
        var dto = new UpdatePointCategoryDto
        {
            Name = "New Name",
            Description = "New description",
            Aggregation = "max"
        };
        _mockPointCategoryRepository.Setup(x => x.GetByIdAsync("xp", It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Act
        var result = await _service.UpdatePointCategoryAsync("xp", dto);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Name.ShouldBe("New Name");
        _mockPointCategoryRepository.Verify(x => x.UpdateAsync(It.IsAny<PointCategory>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeletePointCategoryAsync_WithValidId_ShouldDeleteCategory()
    {
        // Arrange
        _mockPointCategoryRepository.Setup(x => x.ExistsAsync("xp", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.DeletePointCategoryAsync("xp");

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeTrue();
        _mockPointCategoryRepository.Verify(x => x.DeleteAsync("xp", It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Badges Tests

    [Fact]
    public async Task GetAllBadgesAsync_ShouldReturnAllBadges()
    {
        // Arrange
        var badges = new List<Badge>
        {
            new("badge-1", "Badge 1", "Description 1", "/image1.png"),
            new("badge-2", "Badge 2", "Description 2", "/image2.png")
        };
        _mockBadgeRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(badges);

        // Act
        var result = await _service.GetAllBadgesAsync();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Count().ShouldBe(2);
    }

    [Fact]
    public async Task GetVisibleBadgesAsync_ShouldReturnVisibleBadges()
    {
        // Arrange
        var badges = new List<Badge>
        {
            new("badge-1", "Badge 1", "Description 1", "/image1.png", true),
            new("badge-2", "Badge 2", "Description 2", "/image2.png", false)
        };
        _mockBadgeRepository.Setup(x => x.GetVisibleAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(badges.Where(b => b.Visible));

        // Act
        var result = await _service.GetVisibleBadgesAsync();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldHaveSingleItem();
    }

    [Fact]
    public async Task CreateBadgeAsync_WithValidData_ShouldCreateBadge()
    {
        // Arrange
        var dto = new CreateBadgeDto
        {
            Id = "badge-1",
            Name = "Badge 1",
            Description = "Description 1",
            Image = "/image1.png",
            Visible = true
        };
        _mockBadgeRepository.Setup(x => x.ExistsAsync("badge-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _service.CreateBadgeAsync(dto);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Id.ShouldBe("badge-1");
        _mockBadgeRepository.Verify(x => x.AddAsync(It.IsAny<Badge>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Trophies Tests

    [Fact]
    public async Task GetAllTrophiesAsync_ShouldReturnAllTrophies()
    {
        // Arrange
        var trophies = new List<Trophy>
        {
            new("trophy-1", "Trophy 1", "Description 1", "/image1.png"),
            new("trophy-2", "Trophy 2", "Description 2", "/image2.png")
        };
        _mockTrophyRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(trophies);

        // Act
        var result = await _service.GetAllTrophiesAsync();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Count().ShouldBe(2);
    }

    [Fact]
    public async Task CreateTrophyAsync_WithValidData_ShouldCreateTrophy()
    {
        // Arrange
        var dto = new CreateTrophyDto
        {
            Id = "trophy-1",
            Name = "Trophy 1",
            Description = "Description 1",
            Image = "/image1.png",
            Visible = true
        };
        _mockTrophyRepository.Setup(x => x.ExistsAsync("trophy-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _service.CreateTrophyAsync(dto);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Id.ShouldBe("trophy-1");
        _mockTrophyRepository.Verify(x => x.AddAsync(It.IsAny<Trophy>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Levels Tests

    [Fact]
    public async Task GetAllLevelsAsync_ShouldReturnAllLevels()
    {
        // Arrange
        var levels = new List<Level>
        {
            new("bronze", "Bronze", "xp", 100L),
            new("silver", "Silver", "xp", 500L)
        };
        _mockLevelRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(levels);

        // Act
        var result = await _service.GetAllLevelsAsync();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Count().ShouldBe(2);
    }

    [Fact]
    public async Task GetLevelsByCategoryAsync_WithValidCategory_ShouldReturnLevels()
    {
        // Arrange
        var levels = new List<Level>
        {
            new("bronze", "Bronze", "xp", 100L),
            new("silver", "Silver", "xp", 500L)
        };
        _mockLevelRepository.Setup(x => x.GetByCategoryOrderedAsync("xp", It.IsAny<CancellationToken>()))
            .ReturnsAsync(levels);

        // Act
        var result = await _service.GetLevelsByCategoryAsync("xp");

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Count().ShouldBe(2);
    }

    [Fact]
    public async Task GetLevelsByCategoryAsync_WithEmptyCategory_ShouldReturnFailure()
    {
        // Act
        var result = await _service.GetLevelsByCategoryAsync("");

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldContain("Category cannot be empty");
    }

    [Fact]
    public async Task CreateLevelAsync_WithValidData_ShouldCreateLevel()
    {
        // Arrange
        var dto = new CreateLevelDto
        {
            Id = "bronze",
            Name = "Bronze",
            Category = "xp",
            MinPoints = 100L
        };
        _mockLevelRepository.Setup(x => x.ExistsAsync("bronze", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _service.CreateLevelAsync(dto);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Id.ShouldBe("bronze");
        _mockLevelRepository.Verify(x => x.AddAsync(It.IsAny<Level>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion
}
