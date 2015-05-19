
#include <avr/io.h>
#include <avr/interrupt.h>
#include <avr/pgmspace.h>
#include <util/delay.h>
#include <string.h>
#include <stdint.h>
#include <stdlib.h>

#include "rfm12.h"

#define LED_PORT  PORTB
#define LED_DDR    DDRB
#define LED_BIT     PB0 

#define VD_OFF  LED_PORT |= 1 << LED_BIT;
#define VD_ON  LED_PORT &= ~(1 << LED_BIT);
#define VD_TOGGLE LED_PORT ^= 1 << LED_BIT;

#define BAUD        19200UL
#define UBRR_BAUD   ((F_CPU/(16UL*BAUD))-1)

void _blink(void)
{
	//VD_ON;
	//_delay_ms(100);
	//VD_OFF;
	//_delay_ms(100);
	VD_TOGGLE;
}

void uart_init(void)
{
    UBRRH = (uint8_t) (UBRR_BAUD>>8);
    UBRRL = (uint8_t) (UBRR_BAUD & 0x0ff);

    UCSRB = (1<<RXEN)|(1<<TXEN);

    UCSRC = (1<<URSEL)|(1<<UCSZ1)|(1<<UCSZ0);
}

void USARTWriteChar(char data)
{
   while(!(UCSRA & (1<<UDRE)))
   {
      //Do nothing
   }
   //Now write the data to USART buffer
   UDR=data;
}

int main(void)
{
		uint8_t *bufptr;
		uint8_t i;
		
		LED_DDR |= _BV(LED_BIT); //enable LED if any
		VD_OFF;
		
		uart_init();	
			
		_delay_ms(100);  //little delay for the rfm12 to initialize properly
		rfm12_init();    //init the RFM12
		_delay_ms(250);  //little delay for the rfm12 to initialize properly

        sei();           //interrupts on
		
		USARTWriteChar(0xFF);
		
		_blink();
        
        while (1)
        {
			if (rfm12_rx_status() == STATUS_COMPLETE)
			{
				//so we have received a message
				_blink();

				//get the address of the current rx buffer
				bufptr = rfm12_rx_buffer(); 

				USARTWriteChar(0xFF);
				//dump buffer contents to uart			
				for (i=0;i<rfm12_rx_len();i++)
				{
					USARTWriteChar(bufptr[i]);
				}
				USARTWriteChar(0xFA);
				
				
				// tell the implementation that the buffer
				// can be reused for the next data.
				rfm12_rx_clear();
			}
        };
}