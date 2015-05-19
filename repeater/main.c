#include <avr/io.h>
#include <avr/interrupt.h>
#include <avr/pgmspace.h>
#include <util/delay.h>
#include <string.h>
#include <stdint.h>
#include <stdlib.h>
#include <avr/sleep.h>
#include <avr/wdt.h>

#include "rfm12.h"

#define LED_PORT  	PORTB
#define LED_DDR    	DDRB
#define LED_BIT     PB0

#define VD_OFF  	LED_PORT |= 1 << LED_BIT;
#define VD_ON  		LED_PORT &= ~(1 << LED_BIT);

void _blink(void)
{
	VD_ON;
	_delay_ms(10);
	VD_OFF;
}

int main(void)
{
		//uint8_t *bufptr;
		//uint8_t i;
		//uint8_t *buf;
		uint8_t tv[] = {0x0A};
		
		
		LED_DDR |= _BV(LED_BIT); //enable LED if any
		VD_OFF;

		_blink();
		
		_delay_ms(100);  //little delay for the rfm12 to initialize properly
		rfm12_init();    //init the RFM12
		_delay_ms(250);  //little delay for the rfm12 to initialize properly

		// Disable unused modules
		ADCSRA &= ~(1<<ADEN); 	 //Disable ADC
		ACSR |= (1 << ACD);     //Disable Analog Comparator
		wdt_disable();			 //Disable Watchdog Timer

        sei();           //interrupts on
		
		//set the wakeup timer to 4 * (2^8)ms = 1.024s
		rfm12_set_wakeup_timer(0x804);

		//set AVR sleep mode
		set_sleep_mode(SLEEP_MODE_IDLE);
		sleep_enable();
		
		_blink();
        
        while (1)
        {
			//sleep while receiving
			while(!ctrl.wkup_flag) 
			{
				sleep_mode();
				if (rfm12_rx_status() == STATUS_COMPLETE)
				{
					//so we have received a message
					rfm12_rx_buffer();
				
					rfm12_tx (sizeof(tv), 0xEE, tv);
					rfm12_tick();
					rfm12_rx_clear();
					_delay_ms(1);
					_blink();				
				}
			}
			ctrl.wkup_flag = 0;
        };
}