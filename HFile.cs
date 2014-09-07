using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace HexViewer
{
    

    class HFile
    {
        private String path = null;
        public long length { get; set; }
        Stream stream = null;
        public HFile(String fPath) {
            this.path = fPath;

            stream = File.Open(fPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            length = stream.Length;
        }

        ~HFile() {
            if(stream != null)
                stream.Close();
            stream = null;
        }

        public String[] read(int fromLine, int lineLength, int numLines, int grouped = 4) {
            
            if(stream != null && stream.CanRead) {
                StringBuilder sb = new StringBuilder();
                StringBuilder ss = new StringBuilder();
                long pos = ((fromLine) * lineLength);
                
                if(pos > length)
                    return null;

                stream.Position = pos;
                byte[] buffer = new byte[lineLength];
                String[] strings = new String[numLines];
                int offset = fromLine * lineLength;
                int r = 0;
                for (int i = 0; i < numLines;i++) {
                    sb.Clear();
                    ss.Clear();
                    r = stream.Read(buffer, 0, lineLength);
                    sb.AppendFormat("{0:x08}:\t", (i * lineLength)+offset);
                    if (r > 0) {
                        for (int z = 0; z < r; z++) {
                            int b = (int)buffer[z];
                            sb.AppendFormat("{0:x02}", b);
                            if (((z+1) % 4) == 0 && z != 0)
                                sb.Append(" ");
                            if (b == 0)
                                b = 0x20;
                            else if (b < 20)
                                b = 0x3F;
                            ss.AppendFormat("{0}", (char)b);
                        }
                        if (r < lineLength) {
                            for (int z = 0; z < (lineLength - r); z++) {
                                if (((z + 1) % 4) == 0 && z != 0)
                                    sb.Append(" ");
                                sb.Append("  ");
                            }   
                        }
                        sb.AppendFormat("\t\t{0}", ss.ToString());
                        strings[i] = sb.ToString();
                    }
                    else {
                        break;
                    }
                }
                return strings;
            }

            return null;
        }

        public void close() {
            stream.Close();
            stream = null;
        }
    }


}
