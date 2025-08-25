using GamificationEngine.Domain.Events;
using GamificationEngine.Domain.Repositories;
using Shouldly;
using Xunit;

namespace GamificationEngine.Domain.Tests;

public class IEventRepositoryTests
{
    [Fact]
    public void Interface_ShouldDefineStoreAsyncMethod()
    {
        // Arrange & Act
        var methodInfo = typeof(IEventRepository).GetMethod("StoreAsync");

        // Assert
        methodInfo.ShouldNotBeNull();
        methodInfo.ReturnType.ShouldBe(typeof(Task));
        methodInfo.GetParameters().ShouldHaveSingleItem();
        methodInfo.GetParameters()[0].ParameterType.ShouldBe(typeof(Event));
        methodInfo.GetParameters()[0].Name.ShouldBe("event");
    }

    [Fact]
    public void Interface_ShouldDefineGetByUserIdAsyncMethod()
    {
        // Arrange & Act
        var methodInfo = typeof(IEventRepository).GetMethod("GetByUserIdAsync");

        // Assert
        methodInfo.ShouldNotBeNull();
        methodInfo.ReturnType.ShouldBe(typeof(Task<IEnumerable<Event>>));
        methodInfo.GetParameters().Length.ShouldBe(3);

        var parameters = methodInfo.GetParameters();
        parameters[0].ParameterType.ShouldBe(typeof(string));
        parameters[0].Name.ShouldBe("userId");
        parameters[1].ParameterType.ShouldBe(typeof(int));
        parameters[1].Name.ShouldBe("limit");
        parameters[2].ParameterType.ShouldBe(typeof(int));
        parameters[2].Name.ShouldBe("offset");
    }

    [Fact]
    public void Interface_ShouldDefineGetByTypeAsyncMethod()
    {
        // Arrange & Act
        var methodInfo = typeof(IEventRepository).GetMethod("GetByTypeAsync");

        // Assert
        methodInfo.ShouldNotBeNull();
        methodInfo.ReturnType.ShouldBe(typeof(Task<IEnumerable<Event>>));
        methodInfo.GetParameters().Length.ShouldBe(3);

        var parameters = methodInfo.GetParameters();
        parameters[0].ParameterType.ShouldBe(typeof(string));
        parameters[0].Name.ShouldBe("eventType");
        parameters[1].ParameterType.ShouldBe(typeof(int));
        parameters[1].Name.ShouldBe("limit");
        parameters[2].ParameterType.ShouldBe(typeof(int));
        parameters[2].Name.ShouldBe("offset");
    }

    [Fact]
    public void Interface_ShouldDefineGetByIdAsyncMethod()
    {
        // Arrange & Act
        var methodInfo = typeof(IEventRepository).GetMethod("GetByIdAsync");

        // Assert
        methodInfo.ShouldNotBeNull();
        methodInfo.ReturnType.ShouldBe(typeof(Task<Event?>));
        methodInfo.GetParameters().ShouldHaveSingleItem();
        methodInfo.GetParameters()[0].ParameterType.ShouldBe(typeof(string));
        methodInfo.GetParameters()[0].Name.ShouldBe("eventId");
    }

    [Fact]
    public void Interface_ShouldBePublic()
    {
        // Arrange & Act
        var type = typeof(IEventRepository);

        // Assert
        type.IsPublic.ShouldBeTrue();
        type.IsInterface.ShouldBeTrue();
    }

    [Fact]
    public void Interface_ShouldHaveCorrectNamespace()
    {
        // Arrange & Act
        var type = typeof(IEventRepository);

        // Assert
        type.Namespace.ShouldBe("GamificationEngine.Domain.Repositories");
    }

    [Fact]
    public void Interface_ShouldHaveCorrectName()
    {
        // Arrange & Act
        var type = typeof(IEventRepository);

        // Assert
        type.Name.ShouldBe("IEventRepository");
    }

    [Fact]
    public void Interface_ShouldHaveCorrectGenericConstraints()
    {
        // Arrange & Act
        var type = typeof(IEventRepository);

        // Assert
        type.IsGenericType.ShouldBeFalse();
        type.GetGenericArguments().Length.ShouldBe(0);
    }

    [Fact]
    public void Interface_ShouldHaveCorrectMethodSignatures()
    {
        // Arrange
        var type = typeof(IEventRepository);

        // Act & Assert
        var storeMethod = type.GetMethod("StoreAsync");
        var getByUserIdMethod = type.GetMethod("GetByUserIdAsync");
        var getByTypeMethod = type.GetMethod("GetByTypeAsync");
        var getByIdMethod = type.GetMethod("GetByIdAsync");

        storeMethod.ShouldNotBeNull();
        getByUserIdMethod.ShouldNotBeNull();
        getByTypeMethod.ShouldNotBeNull();
        getByIdMethod.ShouldNotBeNull();

        // Verify all methods are async
        storeMethod.ReturnType.ShouldBe(typeof(Task));
        getByUserIdMethod.ReturnType.ShouldBe(typeof(Task<IEnumerable<Event>>));
        getByTypeMethod.ReturnType.ShouldBe(typeof(Task<IEnumerable<Event>>));
        getByIdMethod.ReturnType.ShouldBe(typeof(Task<Event?>));
    }

    [Fact]
    public void Interface_ShouldHaveCorrectParameterTypes()
    {
        // Arrange
        var type = typeof(IEventRepository);

        // Act
        var storeMethod = type.GetMethod("StoreAsync");
        var getByUserIdMethod = type.GetMethod("GetByUserIdAsync");
        var getByTypeMethod = type.GetMethod("GetByTypeAsync");
        var getByIdMethod = type.GetMethod("GetByIdAsync");

        // Assert
        storeMethod.ShouldNotBeNull();
        getByUserIdMethod.ShouldNotBeNull();
        getByTypeMethod.ShouldNotBeNull();
        getByIdMethod.ShouldNotBeNull();

        // StoreAsync parameter
        var storeParams = storeMethod!.GetParameters();
        storeParams.Length.ShouldBe(1);
        storeParams[0].ParameterType.ShouldBe(typeof(Event));

        // GetByUserIdAsync parameters
        var getByUserIdParams = getByUserIdMethod!.GetParameters();
        getByUserIdParams.Length.ShouldBe(3);
        getByUserIdParams[0].ParameterType.ShouldBe(typeof(string));
        getByUserIdParams[1].ParameterType.ShouldBe(typeof(int));
        getByUserIdParams[2].ParameterType.ShouldBe(typeof(int));

        // GetByTypeAsync parameters
        var getByTypeParams = getByTypeMethod!.GetParameters();
        getByTypeParams.Length.ShouldBe(3);
        getByTypeParams[0].ParameterType.ShouldBe(typeof(string));
        getByTypeParams[1].ParameterType.ShouldBe(typeof(int));
        getByTypeParams[2].ParameterType.ShouldBe(typeof(int));

        // GetByIdAsync parameter
        var getByIdParams = getByIdMethod!.GetParameters();
        getByIdParams.Length.ShouldBe(1);
        getByIdParams[0].ParameterType.ShouldBe(typeof(string));
    }
}