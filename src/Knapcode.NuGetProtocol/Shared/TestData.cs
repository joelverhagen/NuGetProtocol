using System;
using System.IO;
using System.Linq;

namespace Knapcode.NuGetProtocol.Shared
{
    public class TestData
    {
        public static TestData InitializeFromRepository()
        {
            var startingDirectory = Directory.GetCurrentDirectory();
            var currentDirectory = startingDirectory;
            while (currentDirectory != null && !Directory
                       .EnumerateFiles(currentDirectory)
                       .Select(x => Path.GetFileName(x))
                       .Contains("Knapcode.NuGetProtocol.sln"))
            {
                currentDirectory = Path.GetDirectoryName(currentDirectory);
            }

            if (currentDirectory == null)
            {
                throw new InvalidOperationException(
                    $"The root of the repository could not be found. The starting directory is:{Environment.NewLine}" +
                    $"{startingDirectory}");
            }

            var artifactsDirectory = Path.Combine(currentDirectory, "artifacts");
            if (!Directory.Exists(artifactsDirectory))
            {
                throw new InvalidOperationException(
                    $"The artifacts directory does not exist. Make sure to run build.ps1. The repository directory is:{Environment.NewLine}" +
                    $"{currentDirectory}");
            }

            return new TestData(artifactsDirectory);
        }

        private readonly string _directory;

        private TestData(string directory)
        {
            _directory = directory;
        }

        public Stream PackageFullMetadata => new FileStream(
            Path.Combine(_directory, "K.Np.FullMetadata.1.0.0-alpha1.nupkg"),
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read);

        public Stream PackageUnlisted => new FileStream(
            Path.Combine(_directory, "K.Np.Unlisted.1.0.0-beta1.nupkg"),
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read);
    }
}
