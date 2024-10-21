variables
{
    const int CANTP_BUFFER_MAX = 4095;
    typedef struct
    {
        int userid;
        int channel;
        int rx_id;
        int tx_id;
        int flags;

        int state;
        byte sn;
        byte receive_bs;

        int blocks_remaining;

        int length;
        int pos;

        Timer transmit_timeout_timer;
        Timer receive_timeout_timer;
        Timer cf_delay_timer;

        int cf_delay_rx;

        byte buffer[CANTP_BUFFER_MAX];

        int transmit_result;
        int receive_result;

        byte wft;
        byte wft_max;

        byte eid;
        byte rx_eid;
        byte tx_eid;

        byte func;
        int mask;
        int code;

        int rx_id_last;
        byte rx_eid_last;
        byte fixed;
        byte fc_enabled;
    } myCanTpInfo;
}

void txIndication(int handle);
void rxIndication(int handle);
void ffIndication(int handle);

void canTpInit(void);
int canTpOpen(int &handle, int rxid, int txid, const char protocol[]);
int canTpOpen(int &handle, int rxid, int txid, const char proocol[],
              int addr_mode_rx);
int canTpOpen(int &handle, int rxid, int txid, const char protocol[],
              int addr_mode_rx, int addr_mode_tx);
void canTpClose(int handle);
int canTpTransmit(int handle, const byte data[]);
int canTpAbort(int handle);
int canTpSetAttr(int handle, int attr, int value);
int canTpGetAttr(int handle, int attr);

void cantp_transmit_ff(int handle);
void cantp_transmit_cf(int handle);
void cantp_transmit_fc(int handle);
