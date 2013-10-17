/*
 * Copyright (C) 1997-2006 by Objective Systems, Inc.
 *
 * This software is furnished under a license and may be used and copied
 * only in accordance with the terms of such license and with the
 * inclusion of the above copyright notice. This software or any other
 * copies thereof may not be provided or otherwise made available to any
 * other person. No title to and ownership of the software is hereby
 * transferred.
 *
 * The information in this software is subject to change without notice
 * and should not be construed as a commitment by Objective Systems, Inc.
 *
 * PROPRIETARY NOTICE
 *
 * This software is an unpublished work subject to a confidentiality agreement
 * and is protected by copyright and trade secret law.  Unauthorized copying,
 * redistribution or other use of this work is prohibited.
 *
 * The above notice of copyright on this source code product does not indicate
 * any actual or intended publication of such source code.
 *
 *****************************************************************************/
/*
////////////////////////////////////////////////////////////////////
//
// BER2INDEF
// 
// Converts the contents of a BER-encoded ASN.1 data file to indefinite 
// length form.
//
// Author Artem Bolgar.
// version 1.05  27 May, 2003
*/
#include "rtbersrc/asn1ber.h"
#include "rtsrc/asn1intl.h"
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#define NUM_SEGMENT_BYTES  12	/* # of bytes to display in a segment */
#define DELTA              16384

int xu_to_indef_len(OSCTXT* ctxt, OSCTXT* destCtxt);

int main (int argc, char** argv)
{
   FILE      *fp, *wp;
   int       len, stat, idx;
   char      *bufp;
   OSCTXT    destCtxt;
   OSCTXT    ctxt;

   if (argc != 3) {
      printf ("usage: ber2indef <filename> <output_filename>\n");
      printf ("  <filename>  Name of file containing BER encoded data\n");
      printf ("  <output_filename>  Name of output file\n");
      return 0;
   }

   if ((fp = fopen (argv[1], "rb")) == 0) {
      perror ("fopen");
      printf ("filename: '%s'\n", argv[1]);
      return -1;
   }

   if ((wp = fopen (argv[2], "wb")) == 0) {
      perror ("fopen");
      printf ("filename: '%s'\n", argv[2]);
      return -1;
   }

   fseek (fp, 0, SEEK_END);
   len = ftell (fp);
   fseek (fp, 0, SEEK_SET);

   bufp = (char*) malloc (len);
   if (fread (bufp, 1, len, fp) != (unsigned int) len) {
      perror ("fread");
      printf ("Can't read file: '%s'\n", argv[2]);
      return -1;
   }
   idx = 0;
   
   while (idx < len) {
      rtInitContext (&ctxt);
      stat = xd_setp (&ctxt, (unsigned char*)bufp + idx, 
                      len - idx, NULL, NULL);
      if(stat != 0) {
         LOG_RTERR(((OSCTXT*)&ctxt), stat);
         rtxErrPrint (&ctxt);
         break;
      }

      stat = xu_to_indef_len (&ctxt, &destCtxt);
      if(stat != 0) {
         LOG_RTERR(((OSCTXT*)&destCtxt), stat);
         rtxErrPrint (&destCtxt);
         break;
      }
      else {
         fwrite (destCtxt.buffer.data, 1, destCtxt.buffer.size, wp);
      }
      idx += ctxt.buffer.byteIndex;
   }
   fclose (wp);
   fclose (fp);
   free (bufp);
   return (0);
}

static void xu_putBuff(OSRTBuffer* buffer_p, void* src, int len) {
   if(!buffer_p->data) {
      buffer_p->size = DELTA;
      buffer_p->data = (OSOCTET*)malloc(buffer_p->size);
   }
   else if(buffer_p->byteIndex + len >= buffer_p->size) {
      buffer_p->size += DELTA + buffer_p->byteIndex + len;
      buffer_p->data = (OSOCTET*)realloc(buffer_p->data, buffer_p->size);
   }
   memcpy(&buffer_p->data[buffer_p->byteIndex], src, len);
   buffer_p->byteIndex += len;
}

int xu_to_indef_len2 (OSCTXT* ctxt, OSRTBuffer* buf_p, 
                      int totalLen, OSCTXT* ectxt) 
{
   ASN1TAG tag;
   int len, curidx = 0;
   int eoc = 0, stat;

   while (curidx < totalLen && xd_tag_len(ctxt, &tag, &len, XM_SKIP) == 0) {
      if(tag == ASN_ID_EOC) {
         /* skip EOC */
         stat = xd_match(ctxt, tag, NULL, XM_ADVANCE);
         if(stat != 0)
            return LOG_RTERR(ctxt, stat);
         curidx += 2;
         continue;
      }
      if(tag & TM_CONS) {
         OSRTBufSave esavedBufferInfo;
         int elLen = 0;
         int bidx = ctxt->buffer.byteIndex;

         if(len == ASN_K_INDEFLEN) {
            /* calculate actual len */
            len = xd_indeflen_ex (ctxt->buffer.data + ctxt->buffer.byteIndex, 
                                  ctxt->buffer.size - ctxt->buffer.byteIndex);
            if(len < 0)
               return LOG_RTERR(ctxt, len);
            stat = xd_match(ctxt, tag, NULL, XM_ADVANCE);
            if(stat != 0)
               return LOG_RTERR(ctxt, stat);
            /* skip length of tag and indicator of indef len */
            len -= (ctxt->buffer.byteIndex - bidx);
         }
         else {
            stat = xd_match(ctxt, tag, NULL, XM_ADVANCE);
            if(stat != 0)
               return LOG_RTERR(ctxt, stat);
         }
         xu_SaveBufferState (ectxt, &esavedBufferInfo);
         elLen = xe_TagAndIndefLen(ectxt, tag, len);
         xu_putBuff(buf_p, &ectxt->buffer.data[ectxt->buffer.byteIndex],
            elLen - len);   
         xu_RestoreBufferState (ectxt, &esavedBufferInfo);
         stat = xu_to_indef_len2(ctxt, buf_p, len, ectxt);
         if(stat != 0)
            return LOG_RTERR(ctxt, stat);
         xu_putBuff(buf_p, &eoc, 2);   
         curidx += (ctxt->buffer.byteIndex - bidx);
      }
      else {
         int bidx = ctxt->buffer.byteIndex; 
         stat = xd_NextElement(ctxt);   
         if(stat != 0)
            return LOG_RTERR(ctxt, stat);
         len = ctxt->buffer.byteIndex - bidx;
         xu_putBuff(buf_p, &ctxt->buffer.data[bidx], len);   
         curidx += len;
      }
   }
   return 0;
}

int xu_to_indef_len(OSCTXT* ctxt, OSCTXT* destCtxt) {
   int stat;
   OSCTXT ectxt;
   
   memset (destCtxt, 0, sizeof(*destCtxt));
   rtInitContext (&ectxt);
   xe_setp (&ectxt, NULL, ctxt->buffer.size);
   stat = xu_to_indef_len2(ctxt, &destCtxt->buffer, ctxt->buffer.size, &ectxt);
   if(stat != 0) {
      memcpy(&destCtxt->errInfo, &ctxt->errInfo, sizeof(ctxt->errInfo));
      return LOG_RTERR(destCtxt, stat);
   }
   destCtxt->buffer.size = destCtxt->buffer.byteIndex;
   return 0;
}
