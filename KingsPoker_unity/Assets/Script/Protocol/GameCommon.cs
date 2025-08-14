//
// generated from "GameCommon.pd"
//


    public enum KICK_REASON : int
    {
        UNKNOWN = 0,
        JUST_KICK = 1,
        ANOTHER_LOGIN = 2,
        BLOCK = 3,
        GAMEMONEY_LOSE_LIMIT = 4,
        USER_LOGOUT = 5,
        NETWORK_STOP = 6,
    }

    public enum USER_TYPE : int
    {
        PLAYER = 0,
        DEALER = 1,
        OBSERVER = 2,
        UNSEATED = 3,
        BOT = 4,
        BREAK_TIME = 5,
    }

    public enum USER_STATUS : int
    {
        USER = 0,
        OPERATOR = 1,
        BLOCKED = 2,
    }

    public enum API_ACCESS_ROLE : int
    {
        GAME = 1,
        ADMIN = 2,
        PUBLISHER = 4,
        ALL = 255,
    }

    public enum API_ACCESS_PERMIT : int
    {
        LEVEL_0 = 0,
        LEVEL_1 = 1,
        LEVEL_2 = 2,
        LEVEL_3 = 3,
    }

    public enum GAME_TYPE : int
    {
        nothing = 0,
        nlh = 1,
        sng = 2,
        plo = 3,
        mtt = 4,
        short_deck = 5,
    }

    public enum CHIP_TYPE : int
    {
        nothing = 0,
        zc = 1,
        dc = 2,
        cc = 3,
    }

    enum RWARD_COUNT_CHECK_TYPE {
        entry = 0,
        userCount = 1,
    };

    public enum TIER_RATE : int
    {
        nothing = 0,
        silver = 1,
        gold = 2,
        diamond = 3,
    }

    public enum NET_LENGTH : int
    {
        STRING_4_LEN = 4,
        STRING_8_LEN = 8,
        STRING_16_LEN = 16,
        STRING_24_LEN = 24,
        STRING_32_LEN = 32,
        STRING_48_LEN = 48,
        STRING_64_LEN = 64,
        STRING_128_LEN = 128,
        STRING_256_LEN = 256,
        STRING_512_LEN = 512,
        STRING_2048_LEN = 2048,
        UUID_STRING_LEN = 36,
        DATETIME_LEN = 64,
        USER_SESSION_ID_LEN = 64,
        BROADCAST_LEN = 64,
        HALL_BC_BET_POSITIONS = 5,
    }

    public enum ROOM_COMMAND : int
    {
        none = 0,
        just_room_created = 5,
        waiting_player = 10,
        ready_to_start = 15,
        wait_to_start_button = 16,
        check_to_start = 17,
        start = 20,
        playing = 22,
        make_results = 25,
        calc_dividend = 30,
        auto_rebuy_free_chip = 31,
        dividend_check = 35,
        donation = 36,
        break_time = 37,
        check_new_members = 40,
        check_tournament_deal = 41,
        end = 49,
    }

    public enum ROOM_USER_STATUS : int
    {
        nothing = 0,
        ingameTable = 1,
        ingame = 2,
        reserved_leave = 3,
        die = 4,
        winner = 5,
    }

    public enum ROOM_USER_STATUS_SEAT : int
    {
        outofroom = 0,
        standing = 1,
        seated = 2,
        standing_up_after_round = 3,
        rest_after_round = 4,
    }

    public enum ROOM_CREATE_OPTION_DESC : int
    {
        level = 0,
        ante = 1,
        bing = 2,
        small_blind = 3,
        blind = 4,
        max = 5,
        buyin = 6,
        buyin_limit = 7,
        buyin_min = 8,
        buyin_max = 9,
        chip_type = 10,
        buyin_type = 11,
        betting_board = 12,
        betting_count = 13,
        raise_count = 14,
        max_seat = 15,
    }

    public enum POKER_BETTYPE : int
    {
        none = 0,
        die = 1,
        bing = 2,
        dadang = 4,
        check = 8,
        ante = 16,
        insurance = 17,
        run_it_twice = 18,
        win = 19,
        call = 32,
        half = 64,
        quarter = 128,
        max = 256,
        allin = 512,
        small = 1024,
        big = 2048,
        bet = 4096,
        raise = 8192,
        pass = 16384,
        change = 32768,
        full = 65536,
        pb = 131072,
        straddle = 262144,
        extra = 524288,
    }

    public enum POKER_USER_STATUS : int
    {
        nothing = 0,
        die = 10,
        winner = 20,
    }

    public enum POKER_FLOW : int
    {
        none = 0,
        set_up = 2,
        set_boss = 5,
        ante = 10,
        blind_bet = 11,
        straddle = 12,
        draw_cards = 15,
        check_show_down = 16,
        bet = 20,
        bet_start = 21,
        betstage_up = 25,
        draw_comm_cards = 26,
        show_down = 40,
        run_it_twice_check = 42,
        run_it_twice = 41,
        insurance_check = 50,
        insurance = 51,
        open_cards = 55,
        end_of_game = 60,
        goto_result = 65,
        betstage_preflop = 70,
        betstage_flop = 71,
        betstage_turn = 72,
        betstage_river = 73,
        betstage_dawn = 80,
        betstage_morning = 81,
        betstage_afternoon = 82,
        betstage_evening = 83,
        betstage_4th = 90,
        betstage_5th = 91,
        betstage_6th = 92,
        betstage_7th = 93,
    }

    public enum TNMT_FLOW : int
    {
        none = 0,
        open = 10,
        start = 20,
        close = 30,
        end = 40,
        cancel = 97,
        cancel_gmt = 98,
        cancel_abnormal = 99,
    }

    public enum TNMT_MAKE_TYPE : int
    {
        auto = 1,
        gmt = 2,
        user = 3,
    }

    public enum TNMT_REWARD_RANK_TYPE : int
    {
        none = 0,
        propotional = 1,
        fixed_table = 2,
    }

    public enum TNMT_GAME_TNMT_RESULT_STATE : int
    {
        none = 0,
        applied = 1,
        eliminated = 2,
        canceled = 3,
    }

    public enum CAFE_STATUS : int
    {
        normal = 0,
        suspended = 1,
        delete_apply = 8,
        deleted = 9,
        op_on = 10,
        op_off = 11,
    }

    public enum CAFE_TIER : int
    {
        bronze = 1,
        silver = 2,
        gold = 3,
        platinum = 4,
        diamond = 5,
    }

    public enum CAFE_TIER_OPTION : int
    {
        open_fee = 0,
        table_max = 1,
        member_max = 2,
        fee_next_time = 3,
        prepaid_time = 4,
        cc = 5,
    }

    public enum CAFE_MEMBER_STATUS : int
    {
        none = 0,
        applied = 1,
        accepted = 2,
        normal = 2,
        not_accepted = 9,
        suspended = 19,
    }

    public enum CAFE_MEMBER_PERMIT : int
    {
        none = 0,
        member = 1,
        manager = 2,
        owner = 3,
    }

    public enum CAFE_JOIN_STATUS : int
    {
        none = 0,
        applied = 1,
        rejected = 2,
        accepted = 3,
    }

    public enum CAFE_NOTICE_TYPE : int
    {
        none = 0,
        cafe_join = 1,
        cafe_chip_request = 2,
    }

    public enum TRANSFER_TYPE : int
    {
        none = 0,
        send = 1,
        receive = 2,
        sell = 3,
        buy = 4,
        borrow = 5,
        sold = 6,
        bought = 7,
        borrowed = 8,
        sell_rejected = 9,
        buy_rejected = 10,
        borrow_rejected = 11,
        sell_cancel = 12,
        buy_cancel = 13,
        borrow_cancel = 14,
        borrow_payback = 15,
        all = 16,
        game_buy_in = 17,
        game_cash_out = 18,
        transfer = 20,
        transferred = 21,
        transfer2 = 22,
        transferred2 = 23,
    }

    public enum CHAT_TYPE : int
    {
        simple = 0,
        system = 1,
        cafe_join = 2,
        cafe_order_chip = 3,
    }

    public enum SIT_OUT : int
    {
        none = 0,
        next_hand = 1,
        next_blind = 2,
    }

    public enum PLAYER_INFO_CHANGE : int
    {
        none = 0,
        player_info_change_passive_rake_hands = 1,
        player_info_change_passive_rake_time = 2,
        player_info_change_stack_removal_hands = 3,
        player_info_change_stack_removal_time = 4,
        player_info_change_time_bank_price = 5,
        player_info_change_surplus_bet_chip = 6,
    }

    public enum PLAYER_INSURANCE_CHOICE : int
    {
        none = 0,
        cancel = 1,
        accept = 2,
    }

    public enum TIME_BANK_COMMAND : int
    {
        none = 0,
        popup = 1,
        count_down = 2,
    }

    public enum TIME_BANK_MODE : int
    {
        none = 0,
        alwaysbank = 1,
        includingbank = 2,
        excludingbank = 3,
        neverbank = 4,
        on = 5,
        off = 6,
    }

    public enum USER_SHOW_CARDS : int
    {
        none = 0,
        left = 1,
        right = 2,
        all = 3,
    }

    public enum MESSAGE_TYPE : int
    {
        none = 0,
        simple = 1,
        notice = 2,
    }

    public enum NOTIFICATION_TYPE : int
    {
        mute_all_push = 0,
        nightly_noti_push = 1,
        cafe_noti_push = 2,
        approve_push = 3,
        chip_approve_push = 4,
        create_delete_push = 5,
        tournament_push = 6,
        stop_operation_push = 7,
        approve_push_2 = 8,
        chip_push = 9,
        stop_operation_owner_push = 10,
    }

    public enum usedType : int
    {
        notUsed = 0,
        used = 1,
        inUse = 2,
    }

    public enum itemType : int
    {
        none = 0,
        free_chip = 1,
        chip = 1,
        gold = 1,
        value_chip = 2,
        point = 2,
        silver = 2,
        ruby = 3,
        package = 10,
        character = 11,
        membership = 12,
        ticket_tnmt = 13,
        ticket_kickout = 14,
        ticket_nick = 15,
        card_front = 20,
        card_back = 21,
    }