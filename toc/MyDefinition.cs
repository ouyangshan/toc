using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using test;
using System.Collections;

namespace test
{

    class test1
    {
        
        //第二层液晶重构命令
        public static byte[] CVHG = new byte[24];
        public static byte[] CVVG = new byte[24];
        public static byte[] CVFG = new byte[24];
        public static byte[] CHHG = new byte[24];
        public static byte[] CHVG = new byte[24];
        public static byte[] CHFG = new byte[24];
        public static byte[] CWHG = new byte[24];
        public static byte[] CWVG = new byte[24];
        public static byte[] CWFG = new byte[24];

        //第三层液晶重构命令
        public static byte[] CVH = new byte[24];
        public static byte[] CVV = new byte[24];
        public static byte[] CVF = new byte[24];
        public static byte[] CHH = new byte[24];
        public static byte[] CHV = new byte[24];
        public static byte[] CHF = new byte[24];
        public static byte[] CWH = new byte[24];
        public static byte[] CWV = new byte[24];
        public static byte[] CWF = new byte[24];

        public byte[][] Instruction = new byte[18][]{CWFG,CVFG,CHFG,CWVG,CVVG,CHVG,CWHG,CVHG,CHHG,
            CWF,CVF,CHF,CWV,CVV,CHV,CWH,CVH,CHH
        };
        public test1()
        {
            Array.Clear(Instruction, 0, 18);
        }
        //单条重构指令，18位，用于生成单个数据位的重构指令时暂存
        public BitArray Instr = new BitArray(18);
        
 
        //AH编码
        public byte[] AH = new byte[24];
        //AV编码
        public byte[] AV = new byte[24];
        //BH编码
        public byte[] BH = new byte[24];
        //BV编码
        public byte[] BV = new byte[24];

        //数据位分配表
        /*每一个数据位的分配情况用一个字节来表达
         *    xxxx xxxx
         *    7654 3210
         * 7：坏点标志，0坏，1正常
         * 6：是否已分配，0已分配，1未分配
         * 5：保留
         * 4：保留
         * 3-0：4位数值表示逻辑运算类型，共可表达16种逻辑运算
         */

        public byte[] Databit_Alloc_Tb = 
        {
            0xc0,0xc0,0xc0,0xc0,0xc0,0xc0,0xc0,0xc0,
            0xc0,0xc0,0xc0,0xc0,0xc0,0xc0,0xc0,0xc0,
            0xc0,0xc0,0xc0,0xc0,0xc0,0xc0,0xc0,0xc0,

            0xc0,0xc0,0xc0,0xc0,0xc0,0xc0,0xc0,0xc0,
            0xc0,0xc0,0xc0,0xc0,0xc0,0xc0,0xc0,0xc0,
            0xc0,0xc0,0xc0,0xc0,0xc0,0xc0,0xc0,0xc0,

            0xc0,0xc0,0xc0,0xc0,0xc0,0xc0,0xc0,0xc0,
            0xc0,0xc0,0xc0,0xc0,0xc0,0xc0,0xc0,0xc0,
            0xc0,0xc0,0xc0,0xc0,0xc0,0xc0,0xc0,0xc0,

            0xc0,0xc0,0xc0,0xc0,0xc0,0xc0,0xc0,0xc0,
            0xc0,0xc0,0xc0,0xc0,0xc0,0xc0,0xc0,0xc0,
            0xc0,0xc0,0xc0,0xc0,0xc0,0xc0,0xc0,0xc0,

            0xc0,0xc0,0xc0,0xc0,0xc0,0xc0,0xc0,0xc0,
            0xc0,0xc0,0xc0,0xc0,0xc0,0xc0,0xc0,0xc0,
            0xc0,0xc0,0xc0,0xc0,0xc0,0xc0,0xc0,0xc0,

            0xc0,0xc0,0xc0,0xc0,0xc0,0xc0,0xc0,0xc0,
            0xc0,0xc0,0xc0,0xc0,0xc0,0xc0,0xc0,0xc0,
            0xc0,0xc0,0xc0,0xc0,0xc0,0xc0,0xc0,0xc0,

            0xc0,0xc0,0xc0,0xc0,0xc0,0xc0,0xc0,0xc0,
            0xc0,0xc0,0xc0,0xc0,0xc0,0xc0,0xc0,0xc0,
            0xc0,0xc0,0xc0,0xc0,0xc0,0xc0,0xc0,0xc0,

            0xc0,0xc0,0xc0,0xc0,0xc0,0xc0,0xc0,0xc0,
            0xc0,0xc0,0xc0,0xc0,0xc0,0xc0,0xc0,0xc0,
            0xc0,0xc0,0xc0,0xc0,0xc0,0xc0,0xc0,0xc0,

        };

        /// <summary>
        /// 标记数据位坏
        /// 将Databit_Alloc_Tb中下标为n的字节的最高位置0
        /// </summary>
        /// <param name="n">数据位序</param>
        /// <returns></returns>
       public bool Invalid_Databit(int n)
        {
           if( n>191)
               return false ;
            Databit_Alloc_Tb[n ] = (byte)(Databit_Alloc_Tb[n ]&0x7f);
            return true;
        }

       /*写第n个数据位的重构指令
        * 输入：instr 18位重构指令；n 为数据位编号，0~191之间。
        * 功能：将instr的18个有效位从低到高、按位写入到18类重构锁在器的第n位，其余位不变。
        * 例： instr  0000 0010 0000 0000 0000 1000, n = 3
        * 结果： CHHG[0]=xxxx 1xxx     CWF[0]=xxxx 1xxx ,其它16个锁存器内容为xxxx 0xxx
        * 其中，下标0 = n/8, 1的位序 = n%8
        * */
       public bool Wr_1_Databit_Instr(BitArray instr, byte n)
        {
            BitArray temp = new BitArray(192);
            byte[] k = new byte[3];
           instr.CopyTo(k,0);
            if ((k[2]>>2>0) | (n < 0) | (n > 191))//输入合法性验证
                return false;
            for(int t=0;t<18;t++)
            {
                if (instr[t]) { 
                    temp = new BitArray(Instruction[t]);//整屏读出
                    temp.Set(n, true);//只写1，不清0。该函数调用前必须将所有重构指令锁存器清零。
                    temp.CopyTo(Instruction[t], 0);//整屏写回
                }

            }
                return true;
        }

        /// <summary>
        /// 分配数据位
        /// 修改数据位分配表Databit_Alloc_Tb，从Start数据位开始，将指定数量的数据位标记为“已分配”，并标记数据位运算类型。注意跳过“坏位”
        /// 从0号数据位开始，依次分配。
        /// </summary>
       /// <param name="Start">起始数据位</param>
       /// <param name="N">数据位数量</param>
       /// <param name="T">运算类型</param>
       /// <returns>true,分配成功；false,分配失败</returns>
       public bool Databit_Alloc(int Start,int N, byte T)
       {
           if ((N > 191)|(T>15))
               return false;//数据位超限 | 运算类型超限
           int cnt = 0;
           byte alloc_en =(byte)(0xBF | T );//1011 xxxx 设为已分配 低四位表示运算类型
           while(cnt<N)
           {
               if (Start > 191)
                   return false;
               if (Databit_Alloc_Tb[Start] >> 6 == 3)
               {
                   Databit_Alloc_Tb[Start] = (byte)(Databit_Alloc_Tb[Start] & alloc_en);
                   cnt++;
               }
               Start++;
           }
               return true;
       }
       
       
        /// <summary>
        /// 处理器重构
        /// </summary>
        /// <param name="N"></param>
        /// <returns></returns>
        public bool Reconfig_Processor()
        {
            return true;
        }
        /// <summary>
        /// 逻辑运算解析成重构指令
        /// </summary>
        /// <param name="table">逻辑运算真值表，9个字节，ASCII码表示，0，1，u三种取值，分别代表无光、v光、h光</param>
        /// <returns name="cmd">返回与逻辑真值表对应的18bits 重构指令</returns>
        public bool Parse_LogicTable(char [] table, out BitArray cmd)
        {
            //if (table.Length != 9)
            //    return false;
            for(int i = 0;i<9;i++)
            {
                switch (table[i])
                {
                    //case '0':

                    //break;
                    case '1':
                        cmd.Set(i, true);

                    break;
                    case 'u':

                    break;
                
                }


            }
            return true;
        }


        /// <summary>
        /// 重构加法器
        /// </summary>
        /// <param name="n">加法位数</param>
        /// <returns></returns>
        public bool Rec_Adder(byte n)
        {
            return true;
        }

        /// <summary>
        /// 重构逻辑运算器
        /// </summary>
        /// <param name="n">逻辑运算位数</param>
        /// <param name="table">逻辑运算真值表，9个字节，ASCII码表示，0，1，u三种取值，分别代表无光、v光、h光</param>
        /// <returns></returns>
        public bool Rec_Logic(byte n, char[] table)
        {
            return true;
        }











    }
}
