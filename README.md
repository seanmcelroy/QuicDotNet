# QuicDotNet
QuicDotNet is a userspace .NET implementation of Google's QUIC protocol for 0-RTT, low-latency,
lower congestion network transport.

This is a quickly-evolving protocol and this library is mostly experimental and slowly maturing
to keep up with the activity.  At the end, this library should provide the layers required to
make HTTP/2 web requests over QUIC as well as write custom transports over QUIC generally.

## Further reading

* What is QUIC? https://en.wikipedia.org/wiki/QUIC
* High-level, older presentation on the benefits of QUIC https://www.ietf.org/proceedings/88/slides/slides-88-tsvarea-10.pdf
* IETF Internet-Draft for QUIC (NEW January 13, 2016) https://tools.ietf.org/html/draft-tsvwg-quic-protocol-02
* QUIC Crypto https://docs.google.com/document/d/1g5nIXAIkN_Y-7XJW5K45IblHd_L2f5LTaDUDwvZ5L6g/edit
* QUIC Wire Layout Specification https://docs.google.com/document/d/1WJvyZflAO2pq77yOLbp9NsGjC1CHetAXV8I0fQe-B_U/edit
* QUIC Design Document and Specification Rationale https://docs.google.com/document/d/1RNHkx_VvKWyWg6Lr8SZ-saqsQx7rFV-ev2jRFUoVD34/edit
* Google QUIC Prototype discussion forum https://groups.google.com/a/chromium.org/forum/#!forum/proto-quic
** CHLO format discussion https://groups.google.com/a/chromium.org/forum/#!topic/proto-quic/wsocYK5kuNY
* Useful blog post by IXIA on high-level QUIC concepts http://www.ixiacom.com/about-us/news-events/corporate-blog/quic-or-you%E2%80%99ll-miss-it

