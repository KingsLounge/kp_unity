
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;


namespace PokerOdds
{
    public class Card
    {
        public string card;
        public long pattern;
        public long number;
        public long strength;
    }


    public class CalculateCardsValue
    {


        static string[] str_jokboName = {
        "ingame_holdem_rank_highcard",
        "ingame_holdem_rank_onepair",
        "ingame_holdem_rank_twopair",
        "ingame_holdem_rank_threeofkind",
        "ingame_holdem_rank_straght",
        "ingame_holdem_rank_flush",
        "ingame_holdem_rank_fullhouse",
        "ingame_holdem_rank_fourofkind",
        "ingame_holdem_rank_straghtflush",
        "ingame_holdem_rank_royalstraghtflush"
    };

        static string[] str_cardNo = {"","",
        "2",
        "3",
        "4",
        "5",
        "6",
        "7",
        "8",
        "9",
        "10",
        "J",
        "Q",
        "K",
        "A"
    };

        static string[] str_cardPt = {"",
        "ingame_holdem_rank_clover",
        "ingame_holdem_rank_heart",
        "ingame_holdem_rank_dia",
        "ingame_holdem_rank_spade"
    };


        // 각각 족보를 이루를 카드의 수.  High card = 0장  One pair = 2장.....
        // High card 0장에서 1장으로 수정 2021-10-07
        // High card 1장에서 0장으로 수정 2022-04-29
        static int[] jokboCardsNo = { 0, 2, 4, 3, 5, 5, 5, 4, 5, 5 };


        /// <summary>
        ///  card_string을 list형식으로 변환한다. 
        /// </summary>
        /// <param name="card_string"></param>
        /// <returns></returns>
        static List<Card> makeList(string card_string)
        {
            var cards = card_string.SplitAndTrimAll(' ', ',');
            var cList = new List<string>(cards);
            for (int i = cList.Count - 1; i >= 0; i--)
            {
                if (string.IsNullOrEmpty(cList[i]))
                {
                    cList.RemoveAt(i);
                }
            }
            cards = cList.ToArray();
            var cards_n = new int[7, 2];

            System.Text.StringBuilder sb = new System.Text.StringBuilder("card_string ");
            for (int i = 0; i < cards.Length; i++)
            {
                sb.AppendFormat("[{0}] ", cards[i]);
            }
            // Debug.LogWarning(sb.ToString());
            for (int i = 0; i < cards.Length; i++)
            {
                var cards_pattern = cards[i].Substring(1, 1);
                var cards_number = cards[i].Substring(0, 1);

                switch (cards_number)
                {
                    case "a":
                        cards_number = "1";
                        break;
                    case "t":
                        cards_number = "10";
                        break;
                    case "j":
                        cards_number = "11";
                        break;
                    case "q":
                        cards_number = "12";
                        break;
                    case "k":
                        cards_number = "13";
                        break;
                }

                switch (cards_pattern)
                {
                    case "c":
                        cards_pattern = "1";
                        break;
                    case "h":
                        cards_pattern = "2";
                        break;
                    case "d":
                        cards_pattern = "3";
                        break;
                    case "s":
                        cards_pattern = "4";
                        break;
                }

                cards_n[i, 0] = Convert.ToInt32(cards_pattern);
                cards_n[i, 1] = Convert.ToInt32(cards_number);//.AsInt();

            }

            List<Card> data = new List<Card> {
              new Card { card = "card1", pattern = cards_n[0, 0], number = cards_n[0, 1], strength = 1 },
              new Card { card = "card2", pattern = cards_n[1, 0], number = cards_n[1, 1], strength = 1 },
              new Card { card = "card3", pattern = cards_n[2, 0], number = cards_n[2, 1], strength = 1 },
              new Card { card = "card4", pattern = cards_n[3, 0], number = cards_n[3, 1], strength = 1 },
              new Card { card = "card5", pattern = cards_n[4, 0], number = cards_n[4, 1], strength = 1 },
              new Card { card = "card6", pattern = cards_n[5, 0], number = cards_n[5, 1], strength = 1 },
              new Card { card = "card7", pattern = cards_n[6, 0], number = cards_n[6, 1], strength = 1 }
        };

            for (int tc = 0; tc < data.Count; tc++)
            {
                if (data[tc].number == 1) data[tc].strength = 14;
                else data[tc].strength = data[tc].number;
            }

            return data;
        }


        public static string makeMadeCardsOnly(string cards_string_, long v)
        {
            v /= 100000000000;
            int c = jokboCardsNo[v];

            int templength = c * 2 + (c - 1);
            if (templength <= 0) return "";
            return cards_string_.Substring(0, templength);
        }


        public static string makeCardsString(List<Card> cards)
        {
            string str = "";

            for (int c = 0; c < cards.Count; c++)
            {
                switch (cards[c].number)
                {
                    case 1: str += "a"; break;
                    case 10: str += "t"; break;
                    case 11: str += "j"; break;
                    case 12: str += "q"; break;
                    case 13: str += "k"; break;
                    default: str += cards[c].number.ToString(); break;
                }

                switch (cards[c].pattern)
                {
                    case 1: str += "c"; break;
                    case 2: str += "h"; break;
                    case 3: str += "d"; break;
                    case 4: str += "s"; break;
                }

                str += " ";
            }

            str = str.TrimEnd(' ');

            return str;
        }

        /// <summary>
        ///  Hole cards와 Community Cards의 조합을 인자로 받아  value를 만들어 리턴한다. 
        ///  
        /// 세부 설명 ) 
        ///                        RANK	    단 위	        kicker        descript
        ///     ROYAL FLUSH         9	100 000 000	    9 ,   X,    00000 ,유저카드높은수(00), 유저카드높은수무늬(0)
        ///     STRAIGHT FLUSH      8	100 000 000	    8 ,   X,    00000 , 유저카드높은수(00), 유저카드높은수무늬(0)
        ///     FOUR OF A KIND      7	100 000 000	    7 ,   X,    족보중높은수(00) , 000 , 유저카드높은수(00), 유저카드높은수무늬(0)
        ///     FULL HOUSE          6	100 000 000	    6 ,   X,    3장인수(00) , 2장인수(00) , 0 , 유저카드높은수(00), 유저카드높은수무늬(0)
        ///     FLUSH               5	100 000 000	    5 ,   X,    족보중높은수(00), 족보중유저카드높은수(00), 족보중높은수무늬(0), 유저카드높은수(00), 유저카드높은수무늬(0)
        ///     STRAIGHT            4   100 000 000     4 ,   X,    족보중높은수(00), 000, 유저카드높은수(00), 유저카드높은수무늬(0)
        ///     THREE OF A KIND     3   100 000 000     3 ,   X,    쓰피카드수(00), 000, 유저카드높은수(00), 유저카드높은수무늬(0)
        ///     TWO PAIR            2   100 000 000     2 ,   X,    투페어높은수(00), 투페어낮은수(00), 0, 유저카드높은수(00), 유저카드높은수무늬(0)
        ///     ONE PAIR            1   100 000 000     1 ,   X,    원페어수(00), 0000, 유저카드높은수(00), 유저카드높은수무늬(0)
        ///     HIGH CARD           0   100          유저카드높은수(00), 유저카드높은수무늬(0) 
        ///     
        /// 
        ///     static int[] jokboCardsNo = { 0, 2, 4, 3, 5, 5, 5, 4, 5, 5 };

        ///     
        /// 예)	                round1 round2       round3      round4      round5      round6
        ///     user1   ts 9d	104	    104	        104	        104	        104	        104
        ///     user2   kc qc	131	    131	        131	        112000131	112000131	513131131
        ///     user3   qh 8d	122	    122	        122	        112000122	112000122	112000122
        ///     user4	2h 5s	54	    54	        54	        54	        54	        105000054
        ///     user5	3h 8h	82	    82	        82	        82	        82	        82
        ///     user6   jc 3s	111	    111	        111	        111	        111	        111
        ///     user7	3c ah	142	    142	        142	        142	        114000142	114000142
        ///     user8	as 7d	144	    107000144	107000144	107000144	214070114	214070114
        ///     comcard	7c 4c qd ad 5c
        ///     
        /// 
        /// 
        ///     키커 ( Kicker )
        /// 
        ///     hand qh 8d
        ///     comcard	7c 4c qd ad 5c      --> qd qc ad 8d 7c
        ///     
        ///          Q OnePair      112000122    
        ///                 -->     112140807122
        ///                 
        /// 
        ///     hand as 7d
        ///     comcard	7c 4c qd ad 5c      --> as ad 7d 7c qd 
        ///     
        ///         A 7 Two Pairs   214070114
        ///                 -->     214070130114
        ///     
        ///     hand as ac
        ///     comcard	7c 4c qd ad 5c      --> as ad ac qd 7c 
        ///     
        ///         a trips         214070114
        ///                 -->     214070130114
        ///     
        ///     qc 3c 2h 3d kc 8c 3h      Three of a kind,3,,,,Q,Clubs       303000121
        ///       
        ///                 -->     3d 3h 3c kc qc 8c 2h                     303141300121
        ///                 



        /// </summary>
        /// <param name="card_string">Hole cards와 Community Cards의 조합. ex) "6h 8s 7s 3d 9h 8h js"카드와 커뮤니티 카드</param>
        /// <returns>	</returns>
        public static long calc(string card_string)
        {
            long ret = 0;


            List<Card> sorted_cards = null;
            List<Card> sorted_2_cards = null;

            //get

            List<Card> data = makeList(card_string);

            //return
            var rank = 0;
            var card1_p = 0L;
            var card1_n = 0L;
            var card2_p = 0L;
            var card2_n = 0L;

            var rankM = 0L;
            var rankU = 0L;
            var rankP = 0L;
            var userMN = 0L;
            var userMP = 0L;

            //참조
            var chkU_in = new string[] { "card1", "card2" };

            var chk_user_ace = (from b in data
                                where chkU_in.Contains(b.card) & b.number == 1
                                select b).FirstOrDefault();

            if (chk_user_ace != null)
            {
                userMN = 14;
                userMP = chk_user_ace.pattern;
            }
            else
            {
                var chk_user = (from b in data
                                orderby b.number descending, b.pattern descending
                                where chkU_in.Contains(b.card)
                                select b).First();

                userMN = chk_user.number;
                userMP = chk_user.pattern;
            }

            //참조
            var chkM_in = new long[] { 1L, 13L, 12L, 11L, 10L };

            //rank

            //무늬 카운트 그룹바이   --> flash 같은 거 처리..
            var pt_query = from b in data
                           orderby b.pattern descending
                           where b.number > 0
                           group b by b.pattern into g
                           let list = g.ToList()
                           select new
                           {
                               pt = g.Key,
                               cnt = list.Count,
                               num = g.Max(p => p.number),
                               strength = g.Min(p => p.number) == 1 ? 14 : g.Max(p => p.number)
                           };

            //숫자 카운트 그룹바이   --> pair 같은거 처리..
            var nm_query = from b in data
                           orderby b.number descending
                           where b.number > 0
                           group b by b.number into g
                           let list = g.ToList()
                           select new
                           {
                               num = g.Key,
                               strength = g.Key == 1 ? 14 : g.Key,
                               cnt = list.Count
                           };


            var tlist = nm_query.ToList();
            // Debug.Log(tlist.ToString());






            //플러쉬 체크
            var chk_rank5 = (from r in pt_query
                             where r.cnt >= 5
                             select r).FirstOrDefault();
            if (chk_rank5 != null) // 플러쉬다.
            {
                rank = 5;
                card1_p = chk_rank5.pt;
                card1_n = chk_rank5.strength == 14 ? 1 : chk_rank5.num;

                var chk_data_rank5 = (from b in data
                                      orderby b.strength descending  // not number 
                                      where b.pattern == card1_p
                                      select b).Take(5);

                var chk_user_rank5 = (from b in chk_data_rank5
                                      orderby b.strength descending // // not number 
                                      where b.pattern == card1_p & chkU_in.Contains(b.card)
                                      select b).FirstOrDefault();
                if (chk_user_rank5 != null)
                {
                    rankU = chk_user_rank5.number;
                }
                rankM = card1_n;
                rankP = card1_p;

                ret = 500000000000 + (cv_ace(rankM) * 1000000000) + (cv_ace(rankU) * 10000000) + (rankP * 1000000) + (cv_ace(userMN) * 10) + (userMP);
            }

            //is 플러쉬
            if (rank == 5)
            {
                //로티플검사

                var chk_rank9_cnt = (from r in data
                                     orderby r.number
                                     where r.pattern == card1_p & chkM_in.Contains(r.number)
                                     select r).Count();
                if (chk_rank9_cnt == 5)
                {
                    rank = 9;
                    ret = 900000000000 + (userMN * 10) + (userMP);
                }
                else//스티플 검사
                {
                    var chk_rank8_cards = from r in data
                                          orderby r.number
                                          where r.pattern == card1_p
                                          select r;
                    var tmp_num = -1L;
                    var chk_rank8_cnt = 1;
                    foreach (var row in chk_rank8_cards)
                    {
                        if ((tmp_num + 1) == row.number)
                        {
                            chk_rank8_cnt++;
                            card1_n = row.number;
                        }
                        else if (chk_rank8_cnt < 5)
                        {
                            chk_rank8_cnt = 1;
                        }
                        tmp_num = row.number;
                    }
                    if (chk_rank8_cnt >= 5)
                    {
                        rank = 8;
                        //card1_n = tmp_num;

                        rankM = card1_n;
                        ret = 800000000000 + (rankM * 1000000000) + (userMN * 10) + (userMP);
                    }
                }

            }
            else
            {

                //check 포카드
                var chk_rank7 = (from r in nm_query
                                 where r.cnt == 4
                                 select r).FirstOrDefault();
                if (chk_rank7 != null)
                {
                    rank = 7;
                    card1_n = chk_rank7.num;

                    rankM = card1_n;

                    ret = 700000000000 + (cv_ace(rankM) * 1000000000) + (cv_ace(userMN) * 10) + (userMP);
                }
                else
                {
                    //check 트리플
                    var chk_rank3_ace = (from r in nm_query
                                         where r.cnt > 2 & r.num == 1
                                         select r).FirstOrDefault();

                    if (chk_rank3_ace != null)//check 트리플 ace
                    {
                        rank = 3;//트리플 ace
                        card1_n = 14;
                        rankM = card1_n;

                        string debug_str_sorted_ards = "";
                        var except_cards = (from b in data
                                            orderby b.strength descending
                                            where b.number != 1
                                            select b).ToList();

                        ret = 300000000000 + (cv_ace(rankM) * 1000000000) + (cv_ace(userMN) * 10) + (userMP);

                        if (except_cards.Count >= 1)    // you have 4th card.
                        {
                            ret += except_cards[0].strength * 10000000;
                        }
                        if (except_cards.Count >= 2)    // you have 5th card.
                        {
                            ret += except_cards[1].strength * 100000;
                        }

                        // for Debug.Log()
                        {
                            sorted_cards = (from b in data
                                            orderby b.pattern descending
                                            where b.number == 1
                                            select b).ToList();

                            sorted_cards.AddRange(except_cards);

                            debug_str_sorted_ards = "sorted_cards = " + makeCardsString(sorted_cards);
                        }

                        //check 원페어 for 풀하우스
                        var chk_rank6 = (from r in nm_query
                                         where (r.cnt > 1 & r.num != 1)
                                         select r).FirstOrDefault();
                        if (chk_rank6 != null)
                        {
                            rank = 6;//풀하우스
                            card2_n = chk_rank6.num;

                            rankM = card1_n;
                            rankU = card2_n;

                            ret = 600000000000 + (cv_ace(rankM) * 1000000000) + (cv_ace(rankU) * 10000000) + (cv_ace(userMN) * 10) + (userMP);
                        }
                    }
                    else
                    {
                        var chk_rank3 = (from r in nm_query
                                         where r.cnt > 2
                                         select r).FirstOrDefault();
                        if (chk_rank3 != null)//check 트리플
                        {
                            rank = 3;//트리플
                            card1_n = chk_rank3.num;
                            rankM = card1_n;

                            string debug_str_sorted_ards = "";
                            var except_cards = (from b in data
                                                orderby b.strength descending
                                                where b.number != card1_n
                                                select b).ToList();

                            ret = 300000000000 + (cv_ace(rankM) * 1000000000) + (cv_ace(userMN) * 10) + (userMP);

                            if (except_cards.Count >= 1)    // you have 4th card.
                            {
                                ret += except_cards[0].strength * 10000000;
                            }
                            if (except_cards.Count >= 2)    // you have 5th card.
                            {
                                ret += except_cards[1].strength * 100000;
                            }

                            // 디버깅을 위한 정보. 
                            {
                                sorted_cards = (from b in data
                                                orderby b.pattern descending
                                                where b.number == card1_n
                                                select b).ToList();

                                sorted_cards.AddRange(except_cards);

                                debug_str_sorted_ards = "sorted_cards = " + makeCardsString(sorted_cards);
                            }


                            //check 원페어 for 풀하우스
                            var chk_rank6 = (from r in nm_query
                                             where (r.cnt > 1 & r.num != card1_n)
                                             orderby r.strength descending
                                             select r).FirstOrDefault();
                            if (chk_rank6 != null)
                            {
                                rank = 6;//풀하우스
                                card2_n = chk_rank6.num;

                                rankM = card1_n;
                                rankU = card2_n;

                                debug_str_sorted_ards = "";

                                ret = 600000000000 + (cv_ace(rankM) * 1000000000) + (cv_ace(rankU) * 10000000) + (cv_ace(userMN) * 10) + (userMP);

                            }

                            // if( debug_str_sorted_ards != "") Debug.Log(debug_str_sorted_ards);

                        }//check 트리플 end


                        if (rank < 6)//풀하우스 아닌경우
                        {
                            //마운틴검사

                            var chk_rank4m_cnt = (from r in nm_query
                                                  orderby r.num
                                                  where chkM_in.Contains(r.num)
                                                  select r).Count();
                            if (chk_rank4m_cnt >= 5)
                            {
                                rank = 4;
                                card1_n = 14;

                                rankM = card1_n;

                                ret = 400000000000 + (rankM * 1000000000) + (userMN * 10) + (userMP);
                            }
                            else//스트레이트 검사
                            {
                                var chk_rank4_cards = (from r in nm_query
                                                       orderby r.num
                                                       select r).ToList();
                                var tmp_num = -1L;
                                var chk_rank4_cnt = 1;
                                var high_num = -1L;
                                foreach (var row in chk_rank4_cards)
                                {
                                    if ((tmp_num + 1) == row.num)
                                    {
                                        chk_rank4_cnt++;
                                        high_num = row.num; // 스트레이트가 되다면 가장 큰 숫자.
                                    }
                                    else if (chk_rank4_cnt < 5)
                                    {
                                        chk_rank4_cnt = 1;
                                    }
                                    else // 5장은 넘었고 연속성은 끊어졌다면 바로 종료.
                                    {
                                        break;
                                    }
                                    tmp_num = row.num;
                                }

                                if (chk_rank4_cnt >= 5)//스트레이트 검사
                                {
                                    rank = 4;
                                    card1_n = high_num;
                                    rankM = card1_n;

                                    ret = 400000000000 + (rankM * 1000000000) + (userMN * 10) + (userMP);
                                }
                                else if (rank < 3)//투페이검사
                                {
                                    var chk_rank2 = (from r in nm_query
                                                     where r.cnt > 1
                                                     orderby r.strength descending
                                                     select r).Take(2);
                                    if (chk_rank2.Count() == 2)
                                    {
                                        rank = 2;
                                        card1_n = (from r in chk_rank2
                                                   orderby r.strength descending
                                                   select r.num).First();
                                        card2_n = (from r in chk_rank2
                                                   orderby r.strength
                                                   select r.num).First();


                                        rankM = cv_ace(card1_n);
                                        rankU = cv_ace(card2_n);

                                        if (rankM < rankU)
                                        {
                                            var tmpMU = rankM;
                                            rankM = rankU;
                                            rankU = tmpMU;

                                        }


                                        ret = 200000000000 + (rankM * 1000000000) + (rankU * 10000000) + (cv_ace(userMN) * 10) + (userMP);

                                        var except_cards = (from r in data
                                                            where r.strength != rankU & r.strength != rankM
                                                            select r).ToList();

                                        if (except_cards.Count >= 1)    // you have 5th card.
                                        {
                                            ret += except_cards[0].strength * 100000;
                                        }


                                        {
                                            sorted_cards = (from r in data      // 투페어중 첫번째 페어
                                                            orderby r.pattern descending
                                                            where r.strength == rankM
                                                            select r).ToList();
                                            sorted_2_cards = (from r in data    // 투페어중 두번째 페어 
                                                              orderby r.pattern descending
                                                              where r.strength == rankU
                                                              select r).ToList();
                                            sorted_cards.AddRange(sorted_2_cards);
                                            sorted_cards.AddRange(except_cards);

                                            // Debug.Log("sorted_cards = " + makeCardsString(sorted_cards));
                                        }

                                    }
                                    else if (chk_rank2.Count() == 1)
                                    {
                                        rank = 1;
                                        card1_n = (from r in chk_rank2
                                                   orderby r.num descending
                                                   select r.num).First();

                                        rankM = card1_n;

                                        ret = 100000000000 + (cv_ace(rankM) * 1000000000) + (cv_ace(userMN) * 10) + (userMP);

                                        var except_cards = (from r in data
                                                            orderby r.strength descending
                                                            where r.number != rankM
                                                            select r).ToList();


                                        if (except_cards.Count >= 1)    // you have 3rd card.
                                        {
                                            ret += except_cards[0].strength * 10000000;
                                        }
                                        if (except_cards.Count >= 2)    // you have 4th card.
                                        {
                                            ret += except_cards[1].strength * 100000;
                                        }
                                        if (except_cards.Count >= 3)    // you have 5th card.
                                        {
                                            ret += except_cards[2].strength * 1000;
                                        }
                                    }
                                    else
                                    {
                                        var strength = (from d in data
                                                        orderby d.strength descending
                                                        select d.strength).First();


                                        ret = (strength * 10) + (userMP);
                                    }
                                }
                            }

                        }

                    }

                }
            }
            //log(ret.ToString());
            return ret;
        }

        //에이스처리
        static long cv_ace(long number)
        {
            if (number == 1)
            {
                number = 14;
            }
            return (long)number;
        }


        public static string[] jokboName(long v)
        {
            string s = "";

            long jokbo = v / 100000000000;
            string firstCardNo = "";
            string firstCardPt = "";
            string secondCardNo = "";
            string secondCardPt = "";
            string myHoleCardNo = "";
            string myHoleCardPt = "";

            long t;
            long v2;

            switch (jokbo)
            {
                case 0: //  high card
                    firstCardNo = str_cardNo[v / 10];
                    firstCardPt = str_cardPt[v % 10];
                    secondCardNo = "";
                    secondCardPt = "";
                    myHoleCardNo = firstCardNo;
                    myHoleCardPt = firstCardPt;
                    break;
                case 1:
                case 3:
                case 4:
                case 7:
                case 8:
                case 9:
                    t = (v - jokbo * 100000000000) / 1000000000;
                    firstCardNo = str_cardNo[t];
                    firstCardPt = "";
                    secondCardNo = "";
                    secondCardPt = "";
                    myHoleCardNo = str_cardNo[(v % 1000) / 10];
                    myHoleCardPt = str_cardPt[(v % 1000) % 10];
                    break;

                case 5:
                    t = (v - jokbo * 100000000000) / 1000000000;
                    firstCardNo = str_cardNo[t];
                    v2 = (v % 1000000000) / 1000000;
                    firstCardPt = str_cardPt[v2 % 10];
                    secondCardNo = "";
                    secondCardPt = "";
                    myHoleCardNo = str_cardNo[(v % 1000) / 10];
                    myHoleCardPt = str_cardPt[(v % 1000) % 10];
                    break;

                case 2:
                case 6:
                    t = (v - jokbo * 100000000000) / 1000000000;
                    firstCardNo = str_cardNo[t];
                    firstCardPt = "";
                    v2 = (v % 1000000000) / 1000000;
                    secondCardNo = str_cardNo[v2 / 10];
                    secondCardPt = "";
                    myHoleCardNo = str_cardNo[(v % 1000) / 10];
                    myHoleCardPt = str_cardPt[(v % 1000) % 10];
                    break;
            }

            string[] result = new[] { str_jokboName[jokbo], firstCardNo, secondCardNo };//, firstCardPt, secondCardNo, secondCardPt, myHoleCardNo, myHoleCardPt };

            //return str_jokboName[jokbo] + "," + firstCardNo + "," + firstCardPt + "," + secondCardNo + "," + secondCardPt + "," + myHoleCardNo + "," + myHoleCardPt;
            return result;
        }




        public static List<Card> sort(string card_string)
        {
            int ret = 0;

            List<Card> sorted_cards = null;
            List<Card> sorted_2_cards = null;


            //get

            List<Card> data = makeList(card_string);
            data.Sort((c1, c2) => { return (c1.number > c2.number) ? -1 : 1; });
            //return
            var rank = 0;
            var card1_p = 0L;
            var card1_n = 0L;
            var card2_p = 0L;
            var card2_n = 0L;

            var rankM = 0L;
            var rankU = 0L;
            var rankP = 0L;

            //참조

            //참조
            var chkM_in = new long[] { 1L, 13L, 12L, 11L, 10L };

            //rank

            //무늬 카운트 그룹바이
            var pt_query = from b in data
                           orderby b.pattern descending
                           where b.number > 0
                           group b by b.pattern into g
                           let list = g.ToList()
                           select new
                           {
                               pt = g.Key,
                               cnt = list.Count,
                               num = g.Max(p => p.number),
                               strength = g.Min(p => p.number) == 1 ? 14 : g.Max(p => p.number)
                           };
            //숫자 카운트 그룹바이
            var nm_query = from b in data
                           orderby b.number descending
                           where b.number > 0
                           group b by b.number into g
                           let list = g.ToList()
                           select new
                           {
                               num = g.Key,
                               strength = g.Key == 1 ? 14 : g.Key,
                               cnt = list.Count
                           };


            //플러쉬 체크
            var chk_rank5 = (from r in pt_query
                             where r.cnt >= 5
                             select r).FirstOrDefault();
            if (chk_rank5 != null) // 플러쉬다.
            {
                rank = 5;
                card1_p = chk_rank5.pt;
                card1_n = chk_rank5.strength == 14 ? 1 : chk_rank5.num;

                sorted_cards = (from b in data
                                orderby b.strength descending // not number 
                                where b.pattern == card1_p
                                select b).ToList();
                var except_cards = (from b in data                      // 위에 서 빠진것만 따로 정리합니다. 
                                    orderby b.number descending
                                    where b.pattern != card1_p
                                    select b).ToList();

                sorted_cards.AddRange(except_cards);

                // string str = makeCardsString(sorted_cards);

                // ret = 500000000000 + (cv_ace(rankM) * 1000000000) + (cv_ace(rankU) * 10000000) + (rankP * 1000000) + (cv_ace(userMN) * 10) + (userMP);
            }

            //is 플러쉬
            if (rank == 5)
            {
                //로티플검사

                var chk_rank9_cnt = (from r in data
                                     orderby r.number
                                     where r.pattern == card1_p & chkM_in.Contains(r.number)
                                     select r).Count();
                if (chk_rank9_cnt == 5)
                {
                    rank = 9;

                    sorted_cards = (from r in data
                                    orderby r.strength descending
                                    where r.pattern == card1_p & chkM_in.Contains(r.number)
                                    select r).ToList();
                    var except_rotiple = (from r in data
                                          where r.pattern != card1_p | !chkM_in.Contains(r.number)
                                          select r).ToList();

                    sorted_cards.AddRange(except_rotiple);

                    // ret = 900000000000 + (userMN * 10) + (userMP);
                }
                else //스티플 검사
                {
                    var chk_rank8_cards = (from r in data
                                           orderby r.number
                                           where r.pattern == card1_p
                                           select r).ToList();
                    var tmp_num = -1L;
                    var chk_rank8_cnt = 1;
                    List<Card> stpCards = new List<Card>();
                    foreach (Card row in chk_rank8_cards)
                    {
                        if ((tmp_num + 1) == row.number)
                        {
                            chk_rank8_cnt++;
                            card1_n = row.number;
                            stpCards.Add(row);
                        }
                        else if (chk_rank8_cnt < 5)
                        {
                            chk_rank8_cnt = 1;
                            stpCards.Clear();
                            stpCards.Add(row);
                        }

                        tmp_num = row.number;
                    }
                    foreach (Card row in stpCards)
                    {
                        chk_rank8_cards.Remove(row);
                    }

                    if (chk_rank8_cnt >= 5)
                    {
                        rank = 8;
                        //card1_n = tmp_num;
                        sorted_cards = (from r in stpCards
                                        orderby r.strength descending
                                        select r
                                        ).ToList();
                        List<Card> notStpCards = new List<Card>();

                        //sorted_cards = (from r in data
                        //                orderby r.strength descending
                        //                where r.pattern == card1_p
                        //                select r).ToList();

                        var except_rotiple = (from r in data
                                              orderby r.strength descending
                                              where !sorted_cards.Contains(r)
                                              select r).ToList();

                        sorted_cards.AddRange(except_rotiple);

                        // rankM = card1_n;
                        // ret = 800000000000 + (rankM * 1000000000) + (userMN * 10) + (userMP);
                    }
                }

            }
            else
            {

                //check 포카드
                var chk_rank7 = (from r in nm_query
                                 where r.cnt == 4
                                 select r).FirstOrDefault();
                if (chk_rank7 != null)
                {
                    rank = 7;

                    sorted_cards = (from b in data
                                    orderby b.pattern descending
                                    where b.number == chk_rank7.num
                                    select b).ToList();

                    var except_rotiple = (from b in data
                                              //orderby b.pattern descending
                                          where b.number != chk_rank7.num
                                          select b).ToList();

                    sorted_cards.AddRange(except_rotiple);

                    // ret = 700000000000 + (cv_ace(rankM) * 1000000000) + (cv_ace(userMN) * 10) + (userMP);
                }
                else
                {
                    //check 트리플
                    var chk_rank3_ace = (from r in nm_query
                                         where r.cnt > 2 & r.num == 1
                                         select r).FirstOrDefault();
                    if (chk_rank3_ace != null)//check 트리플 ace
                    {
                        rank = 3;//트리플 ace
                        card1_n = 14L;
                        rankM = card1_n;

                        sorted_cards = (from b in data
                                        orderby b.pattern descending
                                        where b.number == 1
                                        select b).ToList();

                        var except_rotiple = (from b in data
                                              where b.number != 1
                                              select b).ToList();

                        sorted_cards.AddRange(except_rotiple);


                        ////  ret = 300000000000 + (cv_ace(rankM) * 1000000000) + (cv_ace(userMN) * 10) + (userMP);

                        //check 원페어 for 풀하우스
                        var chk_rank6 = (from r in nm_query
                                         where (r.cnt > 1 & r.num != 1)
                                         select r).FirstOrDefault();
                        if (chk_rank6 != null)
                        {
                            rank = 6;//풀하우스
                            card2_n = chk_rank6.num;

                            rankM = card1_n;
                            rankU = card2_n;

                            sorted_cards = (from b in data
                                            orderby b.pattern descending
                                            where b.strength == card1_n
                                            select b).ToList();

                            sorted_2_cards = (from b in data
                                              orderby b.pattern descending
                                              where b.strength == card2_n
                                              select b).ToList();

                            except_rotiple = (from b in data
                                              where b.strength == card2_n & b.strength == card1_n
                                              select b).ToList();

                            sorted_cards.AddRange(sorted_2_cards);
                            sorted_cards.AddRange(except_rotiple);

                            //// ret = 600000000000 + (cv_ace(rankM) * 1000000000) + (cv_ace(rankU) * 10000) + (cv_ace(userMN) * 10) + (userMP);

                        }

                    }
                    else
                    {
                        var chk_rank3 = (from r in nm_query
                                         where r.cnt > 2
                                         select r).FirstOrDefault();
                        if (chk_rank3 != null)//check 트리플
                        {
                            rank = 3;//트리플
                            card1_n = chk_rank3.num;

                            rankM = card1_n;

                            sorted_cards = (from b in data
                                            orderby b.pattern descending
                                            where b.number == card1_n
                                            select b).ToList();

                            var except_rotiple = (from b in data
                                                  where b.number != card1_n
                                                  select b).ToList();

                            sorted_cards.AddRange(except_rotiple);


                            //// ret = 300000000000 + (cv_ace(rankM) * 1000000000) + (cv_ace(userMN) * 10) + (userMP);

                            //check 원페어 for 풀하우스
                            var chk_rank6 = (from r in nm_query
                                             where (r.cnt > 1 & r.num != card1_n)
                                             orderby r.strength descending
                                             select r).FirstOrDefault();
                            if (chk_rank6 != null)
                            {
                                rank = 6;//풀하우스
                                card2_n = chk_rank6.num;

                                rankM = card1_n;
                                rankU = card2_n;

                                sorted_cards = (from b in data
                                                orderby b.pattern descending
                                                where b.number == card1_n
                                                select b).ToList();

                                sorted_2_cards = (from b in data
                                                  orderby b.pattern descending
                                                  where b.number == card2_n
                                                  select b).ToList();

                                except_rotiple = (from b in data
                                                  where b.number != card2_n & b.number != card1_n
                                                  select b).ToList();

                                sorted_cards.AddRange(sorted_2_cards);
                                sorted_cards.AddRange(except_rotiple);


                                //// ret = 600000000000 + (cv_ace(rankM) * 1000000000) + (cv_ace(rankU) * 10000000) + (cv_ace(userMN) * 10) + (userMP);

                            }
                        }//check 트리플 end


                        if (rank < 6)//풀하우스 아닌경우
                        {
                            //마운틴검사  ( A,K,Q,J,10 ) 

                            var chk_rank4m_cnt = (from r in nm_query
                                                  orderby r.num
                                                  where chkM_in.Contains(r.num)
                                                  select r).Count();
                            if (chk_rank4m_cnt >= 5)
                            {
                                rank = 4;
                                card1_n = 14;

                                rankM = card1_n;


                                sorted_cards = new List<Card>();
                                for (long i = card1_n; i > card1_n - 5; i--)
                                {
                                    Card t = (from r in data
                                              orderby r.pattern descending
                                              where r.strength == i
                                              select r).First();

                                    sorted_cards.Add(t);
                                }

                                var except_rotiple = (from r in data
                                                      where !sorted_cards.Contains(r)
                                                      select r).ToList();

                                sorted_cards.AddRange(except_rotiple);


                                //// ret = 400000000000 + (rankM * 1000000000) + (userMN * 10) + (userMP);
                            }
                            else//스트레이트 검사
                            {
                                var chk_rank4_cards = (from r in nm_query
                                                       orderby r.num
                                                       select r).ToList();
                                var tmp_num = -1L;
                                var chk_rank4_cnt = 1;

                                var high_num = -1L;
                                foreach (var row in chk_rank4_cards)
                                {
                                    if ((tmp_num + 1) == row.num)
                                    {
                                        chk_rank4_cnt++;
                                        high_num = row.num; // 스트레이트가 되다면 가장 큰 숫자.
                                    }
                                    else if (chk_rank4_cnt < 5)
                                    {
                                        chk_rank4_cnt = 1;
                                    }
                                    else // 5장은 넘었고 연속성은 끊어졌다면 바로 종료.
                                    {
                                        break;
                                    }
                                    tmp_num = row.num;
                                }

                                if (chk_rank4_cnt >= 5)//스트레이트 검사
                                {
                                    rank = 4;
                                    card1_n = high_num;
                                    rankM = card1_n;

                                    sorted_cards = new List<Card>();
                                    for (long i = card1_n; i > card1_n - 5; i--)
                                    {
                                        Card t = (from r in data
                                                  orderby r.pattern descending
                                                  where r.number == i
                                                  select r).First();

                                        sorted_cards.Add(t);
                                    }

                                    var except_rotiple = (from r in data
                                                          where !sorted_cards.Contains(r)
                                                          select r).ToList();

                                    sorted_cards.AddRange(except_rotiple);

                                    //// ret = 400000000000 + (rankM * 1000000000) + (userMN * 10) + (userMP);
                                }
                                else if (rank < 3)//투페이검사
                                {
                                    var chk_rank2 = (from r in nm_query
                                                     where r.cnt > 1
                                                     orderby r.strength descending
                                                     select r).Take(2);
                                    if (chk_rank2.Count() == 2)
                                    {
                                        rank = 2;
                                        card1_n = (from r in chk_rank2
                                                   orderby r.strength descending
                                                   select r.num).First();
                                        card2_n = (from r in chk_rank2
                                                   orderby r.strength
                                                   select r.num).First();

                                        rankM = cv_ace(card1_n);
                                        rankU = cv_ace(card2_n);

                                        if (rankM < rankU)
                                        {
                                            var tmpMU = rankM;
                                            rankM = rankU;
                                            rankU = tmpMU;

                                        }

                                        sorted_cards = (from r in data      // 투페어중 첫번째 페어
                                                        orderby r.pattern descending
                                                        where r.strength == rankM
                                                        select r).ToList();
                                        sorted_2_cards = (from r in data    // 투페어중 두번째 페어 
                                                          orderby r.pattern descending
                                                          where r.strength == rankU
                                                          select r).ToList();
                                        var except_rotiple = (from r in data
                                                              where r.strength != rankU & r.strength != rankM
                                                              select r).ToList();

                                        sorted_cards.AddRange(sorted_2_cards);
                                        sorted_cards.AddRange(except_rotiple);

                                        //// ret = 200000000000 + (rankM * 1000000000) + (rankU * 10000000) + (cv_ace(userMN) * 10) + (userMP);
                                    }
                                    else if (chk_rank2.Count() == 1)
                                    {
                                        rank = 1;
                                        card1_n = (from r in chk_rank2
                                                   orderby r.num descending
                                                   select r.num).First();

                                        rankM = card1_n;

                                        sorted_cards = (from r in data      // 첫번째 페어
                                                        orderby r.pattern descending
                                                        where r.number == rankM
                                                        select r).ToList();
                                        var except_rotiple = (from r in data
                                                              where r.number != rankM
                                                              select r).ToList();

                                        sorted_cards.AddRange(except_rotiple);

                                        //// ret = 100000000000 + (cv_ace(rankM) * 1000000000) + (cv_ace(userMN) * 10) + (userMP);
                                    }
                                    else
                                    {

                                        sorted_cards = (from r in data orderby r.strength descending select r).ToList();

                                        //// ret = (cv_ace(userMN) * 10) + (userMP);
                                    }
                                }
                            }

                        }

                    }

                }
            }

            return sorted_cards;
        }


    }

}