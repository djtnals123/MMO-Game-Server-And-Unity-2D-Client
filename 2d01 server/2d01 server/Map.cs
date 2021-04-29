using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2d01_server
{
    class Map
    {
        private Dictionary<string, ClientInfo> m_userList = new Dictionary<string, ClientInfo>();
        private int m_mapCode;

        public bool test = false;

        public Map(int mapCode)
        {
            m_mapCode = mapCode;
        }

        public Dictionary<string, ClientInfo> UserList
        {
            get
            {
                return m_userList;
            }
            set
            {
                m_userList = value;
            }
        }
        public int MapCode
        {
            get
            {
                return m_mapCode;
            }
            set
            {
                m_mapCode = value;
            }
        }
    }
}
