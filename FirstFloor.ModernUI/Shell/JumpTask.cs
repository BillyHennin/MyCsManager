// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from FirstFloor.ModernUI INC. team.
//  
// Copyrights (c) 2014 FirstFloor.ModernUI INC. All rights reserved.

namespace FirstFloor.ModernUI.Shell
{
    public class JumpTask : JumpItem
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string ApplicationPath { get; set; }
        public string Arguments { get; set; }
        public string WorkingDirectory { get; set; }
        public string IconResourcePath { get; set; }
        public int IconResourceIndex { get; set; }
    }
}