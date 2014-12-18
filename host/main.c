
#include <avr/io.h>
#include <avr/interrupt.h>
#include <avr/pgmspace.h>
#include <util/delay.h>
#include <string.h>
#include <stdint.h>

#include "rfm12.h"
#include "../../libs/uartlibrary/uart.h"

#define LED_PORT  PORTB
#define LED_DDR    DDRB
#define LED_BIT     PB0 

#define VD_OFF  LED_PORT |= 1 << LED_BIT;
#define VD_ON  LED_PORT &= ~(1 << LED_BIT);

#define UART_BAUD_RATE 9600
#define UART_HEXDUMP


void _blink(void)
{
	VD_ON;
	_delay_ms(100);
	VD_OFF;
	_delay_ms(100);
}


int main(void)
{

		uint8_t *bufptr;
		uint8_t i;
	
		LED_DDR |= _BV(LED_BIT); //enable LED if any
	
	
		//uart_init( UART_BAUD_SELECT(UART_BAUD_RATE,F_CPU) ); 
	
		_delay_ms(100);  //little delay for the rfm12 to initialize properly
		rfm12_init();    //init the RFM12

        
        sei();           //interrupts on
		
		//uart_puts ("\r\n" "RFM12 Pingpong test\r\n");
		
		//LED_PORT ^= _BV(LED_BIT);
		
		_blink();
        
        while (1)
        {
			//_blink();
			if (rfm12_rx_status() == STATUS_COMPLETE)
			{
				//so we have received a message

				//blink the LED
				_blink();

				//uart_puts ("new packet: \"");

				bufptr = rfm12_rx_buffer(); //get the address of the current rx buffer

				//dump buffer contents to uart			
				for (i=0;i<rfm12_rx_len();i++)
				{
					//uart_putc ( bufptr[i] );
				}
				
				//uart_puts ("\"\r\n");
				
				// tell the implementation that the buffer
				// can be reused for the next data.
				rfm12_rx_clear();
			}

			//rfm12 needs to be called from your main loop periodically.
			//it checks if the rf channel is free (no one else transmitting), and then
			//sends packets, that have been queued by rfm12_tx above.
			//rfm12_tick();
			
			//_delay_us(100); //small delay so loop doesn't run as fast 		
        };
}