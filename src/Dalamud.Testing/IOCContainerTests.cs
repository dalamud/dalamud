using Dalamud.IOC;
using Xunit;

namespace Dalamud.Testing
{
    class UnversionedDep
    {
    }

    [DependencyVersion( "1.2.3.4" )]
    class VersionedDep
    {
    }

    class UnversionedConsumer
    {
        public UnversionedConsumer(
            VersionedDep ud
        )
        {
        }
    }
    
    class UnversionedConsumerWithUnversionedDep
    {
        public UnversionedConsumerWithUnversionedDep(
            UnversionedDep ud
        )
        {
        }
    }

    class VersionedConsumerVersionedIncorrectly
    {
        public VersionedConsumerVersionedIncorrectly(
            [RequiredVersion("69.420")] VersionedDep dep
        )
        {
        }
    }

    class VersionedConsumerVersionedCorrectly
    {
        public VersionedConsumerVersionedCorrectly(
            [RequiredVersion("1.2.3.4")] VersionedDep dep
        )
        {
        }
    }

    // ReSharper disable once InconsistentNaming
    public class IOCContainerTests
    {
        [Fact]
        public void UnversionedInjectsCorrectly()
        {
            var container = new Container();
            container.RegisterSingleton(new UnversionedDep());
            container.RegisterSingleton(new VersionedDep());

            var instance = container.Create< UnversionedConsumer >();
            Assert.NotNull( instance );
        }

        [Fact]
        public void UnversionedConsumerInjectsVersionedDepSuccessfully()
        {
            var container = new Container();
            container.RegisterSingleton(new UnversionedDep());
            container.RegisterSingleton(new VersionedDep());

            var instance = container.Create< UnversionedConsumer >();
            Assert.NotNull( instance );
        }
        
        [Fact]
        public void UnversionedEverythingInjectsSuccessfully()
        {
            var container = new Container();
            container.RegisterSingleton(new UnversionedDep());
            container.RegisterSingleton(new VersionedDep());

            var instance = container.Create< UnversionedConsumerWithUnversionedDep >();
            Assert.NotNull( instance );
        }

        [Fact]
        public void VersionedConsumerAndDependencyInjectsSuccessfully()
        {
            var container = new Container();
            container.RegisterSingleton(new UnversionedDep());
            container.RegisterSingleton(new VersionedDep());

            var instance = container.Create< VersionedConsumerVersionedCorrectly >();
            Assert.NotNull( instance );
        }

        [Fact]
        public void MismatchingVersionFailsToInject()
        {
            var container = new Container();
            container.RegisterSingleton(new UnversionedDep());
            container.RegisterSingleton(new VersionedDep());

            var instance = container.Create< VersionedConsumerVersionedIncorrectly >();
            Assert.Null( instance );
        }
    }
}
