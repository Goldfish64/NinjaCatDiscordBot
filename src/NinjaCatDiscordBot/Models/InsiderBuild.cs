/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
* File: InsiderBuild.cs
* 
* Copyright (c) 2026 John Davis
*
* Permission is hereby granted, free of charge, to any person obtaining a
* copy of this software and associated documentation files (the "Software"),
* to deal in the Software without restriction, including without limitation
* the rights to use, copy, modify, merge, publish, distribute, sublicense,
* and/or sell copies of the Software, and to permit persons to whom the
* Software is furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included
* in all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
* OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
* THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
* FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
* IN THE SOFTWARE.
* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

using System.Collections.Generic;

namespace NinjaCatDiscordBot.Models {
  /// <summary>
  /// Specifies the type of Insider build.
  /// </summary>
  public enum InsiderBuildType {
    Experimental,
    ExperimentalFuturePlatforms,
    Beta,
    Server,

    // Release-specific.
    Experimental26H1,
    ReleasePreview26H1,
    ReleasePreview24H2_25H2
  }

  /// <summary>
  /// Represents an Insider build.
  /// </summary>
  public class InsiderBuild {
    /// <summary>
    /// Specifies the build type names.
    /// </summary>
    public static readonly Dictionary<InsiderBuildType, string> Names = new() {
      { InsiderBuildType.Experimental, "Experimental" },
      { InsiderBuildType.ExperimentalFuturePlatforms, "Experimental (Future Platforms)" },
      { InsiderBuildType.Beta, "Beta" },
      { InsiderBuildType.Server, "Server" },

      // Release-specific.
      { InsiderBuildType.Experimental26H1, "Experimental (26H1)" },
      { InsiderBuildType.ReleasePreview26H1, "Release Preview 24H2/25H2" },
      { InsiderBuildType.ReleasePreview24H2_25H2, "Release Preview 26H1" },
    };

    /// <summary>
    /// Specifies the build type emotes.
    /// </summary>
    public static readonly Dictionary<InsiderBuildType, string> Emotes = new() {
      { InsiderBuildType.Experimental, "test_tube" },
      { InsiderBuildType.ExperimentalFuturePlatforms, "test_tube" },
      { InsiderBuildType.Beta, "paintbrush" },
      { InsiderBuildType.Server, "desktop" },

      // Release-specific.
      { InsiderBuildType.Experimental26H1, "test_tube" },
      { InsiderBuildType.ReleasePreview26H1, "package" },
      { InsiderBuildType.ReleasePreview24H2_25H2, "package" },
    };

    /// <summary>
    /// Specifies the build type roles.
    /// </summary>
    public static readonly Dictionary<InsiderBuildType, RoleType> Roles = new() {
      { InsiderBuildType.Experimental, RoleType.InsiderExperimental },
      { InsiderBuildType.ExperimentalFuturePlatforms, RoleType.InsiderExperimental },
      { InsiderBuildType.Beta, RoleType.InsiderBeta },
      { InsiderBuildType.Server, RoleType.None },

      // Release-specific.
      { InsiderBuildType.Experimental26H1, RoleType.InsiderExperimental },
      { InsiderBuildType.ReleasePreview26H1, RoleType.InsiderReleasePreview },
      { InsiderBuildType.ReleasePreview24H2_25H2,RoleType.InsiderReleasePreview },
    };

    /// <summary>
    /// Gets or sets the type of Insider build.
    /// </summary>
    public InsiderBuildType Type { get; set; }

    /// <summary>
    /// Gets or sets the build number.
    /// </summary>
    public string BuildNumber { get; set; }

    /// <summary>
    /// Gets or sets the full link to the build release notes.
    /// </summary>
    public string Link { get; set; }
  }
}
