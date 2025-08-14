using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;

public class RoomStatus
{
    public JObject info;

    public long gtn = 0;
    public GAME_TYPE game_type = GAME_TYPE.nlh; // pkr1 holdem
    public CHIP_TYPE chip_type = CHIP_TYPE.nothing;
    public int flow;
    public string ttl = "";
    public bool pw = false;
    public long sm = 0; // 스몰블라인드
    public long bg = 0; // 빅블라이드
    public long an = 0; // 안티
    public long bi = 0; // 바이인 최소
    public long bil = 0; // 바이인 최대
    public string nk = "";
    public string gid = "";
    public long mx = 0; // 맥스베팅 (한국)
    public long emn = 0; // 입장 최소 금액 (한국)
    public long emx = 0; // 입장 최대 금액 (한국)
    public int lv = 0;
    public int tn = 0;
    public long hall = 0;
    public bool random_sit_in = false;
    public bool timebank = false;
    public bool passive_rake = false;
    public int passive_rake_select = 0;
    public long pr1 = 0;
    public long pr2 = 0;
    public long pr3 = 0;
    public long pr4 = 0;

    public bool stack_removal = false;
    public long stack_removal_option1 = 0;
    public long stack_removal_option2 = 0;
    public long stack_removal_bb1 = 0;
    public long stack_removal_bb2 = 0;


    public bool tb2 = false;
    public int tb2_time = 0;
    public int tb2_price = 0;

    public int personnel = 9;

    public long gn;
    public long cafeIdx;
    public int room_command;
    public int holdem_command;
    public int boss_seat;
    public string[] commcards;
    public int who_turn;
    public int who_turn_seat;
    public int turn_nexttime;
    public long money_total;
    public long money_call;
    public long money_minimum_raise;
    public List<RoomUserData> userList = null;
    public JArray playerList = null;

    public bool isObserve = false;
}

public class RoomUserData
{
    public string gid;
    public string cards;
    public int money_bet_thistime;
    public int money_total;
    public int last_bettype;
    public int bettype;
    public string nick;
    public int icon_no;
    public int status;
    public string photourl;
    public long gc;
    public int seat;
    public bool useUrlPhoto = false;
    public JObject time_bank;
}


