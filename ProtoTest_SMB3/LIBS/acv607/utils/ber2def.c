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
//////////////////////////////////////////////////////////////////////
//
// BER2DEF
// 
// Converts the contents of a BER-encoded ASN.1 data file to definite 
// length form.
//
// Author Artem Bolgar.
// version 1.04  8 Dec, 2001
*/
#include "rtbersrc/asn1ber.h"
#include "rtsrc/asn1intl.h"
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#define NUM_SEGMENT_BYTES  12	/* # of bytes to display in a segment */
#define DELTA              16384

int xu_to_def_len(OSCTXT* ctxt, OSCTXT* destCtxt);

typedef struct {
   int idx;
   ASN1TAG tag;
   int len;
} BLOCK;

int main (int argc, char** argv)
{
   FILE      *fp, *wp;
   int       len, stat, idx;
   char      *bufp;
   OSCTXT    destCtxt;
   OSCTXT    ctxt;

   if (argc != 3) {
      printf ("usage: ber2def <filename> <output_filename>\n");
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
      stat = xd_setp (&ctxt, (unsigned char*)bufp + idx, len - idx, NULL, NULL);
      if(stat != 0) {
         LOG_RTERR(((OSCTXT*)&ctxt), stat);
         rtxErrPrint (&ctxt);
         break;
      }

      stat = xu_to_def_len (&ctxt, &destCtxt);
      if(stat != 0) {
         LOG_RTERR(((OSCTXT*)&destCtxt), stat);
         rtxErrPrint (&destCtxt);
         break;
      }
      else {
         int len = destCtxt.buffer.size - destCtxt.buffer.byteIndex - 1;
         fwrite (destCtxt.buffer.data + destCtxt.buffer.byteIndex, 1, len, wp);
         idx += ctxt.buffer.byteIndex;
      }
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

static int xu_make_ref_list(OSRTBuffer* buf, OSCTXT* ctxt, int totalLen) {
   ASN1TAG tag;
   int len, curidx = 0, realLen = 0;
   int eoc = 0, stat;

   while(curidx < totalLen && xd_tag_len(ctxt, &tag, &len, XM_SKIP) == 0) {
      if(tag & TM_CONS) {
         int bidx = ctxt->buffer.byteIndex;
         BLOCK block;
         int iniLen = len, cnt;

         if((tag&0xFF) == 64)
            tag = tag;
         if(len == ASN_K_INDEFLEN) {
            // calculate actual len
            len = xd_indeflen(ctxt->buffer.data + ctxt->buffer.byteIndex);
            if(len < 0)
               return LOG_RTERR(ctxt, len);
            stat = xd_match(ctxt, tag, NULL, XM_ADVANCE);
            if(stat != 0)
               return LOG_RTERR(ctxt, stat);
            // skip length of tag and indicator of indef len
            len -= (ctxt->buffer.byteIndex - bidx);

            //skip EOC
            len -= 2;
         }
         else {
            stat = xd_match(ctxt, tag, NULL, XM_ADVANCE);
            if(stat != 0)
               return LOG_RTERR(ctxt, stat);
         }
         cnt = xu_make_ref_list(buf, ctxt, len);
         if(cnt < 0)
            return LOG_RTERR(ctxt, cnt);
         
         block.idx = ctxt->buffer.byteIndex;
         block.tag = tag;
         block.len = cnt;
         xu_putBuff(buf, &block, sizeof(block));

         if(iniLen == ASN_K_INDEFLEN) {
            stat = xd_match(ctxt, 0, NULL, XM_ADVANCE);
            if(stat != 0)
               return LOG_RTERR(ctxt, stat);
         }
         curidx += (ctxt->buffer.byteIndex - bidx);
         realLen ++;
      }
      else {
         BLOCK block;
         int bidx = ctxt->buffer.byteIndex; 

         stat = xd_NextElement(ctxt);   
         if(stat != 0)
            return LOG_RTERR(ctxt, stat);

         block.idx = ctxt->buffer.byteIndex;
         block.tag = tag;
         block.len = len;
         xu_putBuff(buf, &block, sizeof(block));
         curidx += (ctxt->buffer.byteIndex - bidx);
         realLen ++;
      }
   }
   return realLen;
}

static int xu_to_def_len2(OSCTXT* ctxt, int totalLen, OSCTXT* ectxt, BLOCK *ref, int* refSize) {
   int i, realLen = 0;
   for(i = *refSize - 1; i >= 0 && totalLen; i--) {
      int off = ref[i].idx;
      int len = ref[i].len;
      int stat;
      ASN1TAG tag = ref[i].tag;

      totalLen --;
      if(!(tag & TM_CONS)) {
         stat = xe_memcpy(ectxt, ctxt->buffer.data + off - len, len);
         if(stat < 0)
            return LOG_RTERR(ctxt, stat);
         }
      else {
         len = xu_to_def_len2(ctxt, len, ectxt, ref, &i);
         if(len < 0)
            return LOG_RTERR(ctxt, len);
      }
      stat = xe_tag_len(ectxt, tag, len);
      if(stat < 0)
         return LOG_RTERR(ctxt, stat);

      realLen += stat;
      *refSize = i;
   } 
   return realLen;
}

int xu_to_def_len(OSCTXT* ctxt, OSCTXT* ectxt) {
   int stat;
   OSRTBuffer refBuf;
   OSRTBufSave savedBufferInfo, savedBufferInfo2;
   BLOCK* ref;
   int refSize;

   memset (&refBuf, 0, sizeof(refBuf));
   rtInitContext (ectxt);
   xe_setp (ectxt, NULL, ctxt->buffer.size);

   xu_SaveBufferState (ctxt, &savedBufferInfo);
   stat = xu_make_ref_list(&refBuf, ctxt, ctxt->buffer.size);
   if(stat < 0) {
      memcpy(&ectxt->errInfo, &ctxt->errInfo, sizeof(ctxt->errInfo));
      return LOG_RTERR(ectxt, stat);
   }
   xu_SaveBufferState (ctxt, &savedBufferInfo2);
   xu_RestoreBufferState (ctxt, &savedBufferInfo);
   ref = (BLOCK*) refBuf.data;
   refSize = refBuf.byteIndex/sizeof(BLOCK);

   stat = xu_to_def_len2(ctxt, ctxt->buffer.size, ectxt, ref, &refSize);
   if(stat < 0) {
      memcpy(&ectxt->errInfo, &ctxt->errInfo, sizeof(ctxt->errInfo));
      return LOG_RTERR(ectxt, stat);
   }
   xu_RestoreBufferState (ctxt, &savedBufferInfo2);
   return 0;
}
