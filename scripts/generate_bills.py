"""Generate 5 sample business trip bill/receipt images as PNGs."""
from PIL import Image, ImageDraw, ImageFont
import os

OUTPUT_DIR = os.path.join(os.path.dirname(__file__), "..", "SamplesHR.Backend", "Resources", "Bills")
os.makedirs(OUTPUT_DIR, exist_ok=True)

BILLS = [
    {
        "id": 1,
        "receipt_no": "TX-2026-04417",
        "vendor": "City Taxi Co.",
        "address": "123 Main Street, Seattle, WA",
        "date": "March 3, 2026",
        "items": [("Airport → Hotel transfer", "$45.00")],
        "total": "$45.00",
        "category": "Transportation",
        "payment": "Corporate Card ****4821",
    },
    {
        "id": 2,
        "receipt_no": "GP-2026-18832",
        "vendor": "Grand Plaza Hotel",
        "address": "500 Convention Way, Seattle, WA",
        "date": "March 4-6, 2026",
        "items": [
            ("Deluxe Room - Night 1 (Mar 4)", "$160.00"),
            ("Deluxe Room - Night 2 (Mar 5)", "$160.00"),
        ],
        "total": "$320.00",
        "category": "Accommodation",
        "payment": "Corporate Card ****4821",
    },
    {
        "id": 3,
        "receipt_no": "LB-2026-07741",
        "vendor": "La Bella Cucina",
        "address": "88 Waterfront Ave, Seattle, WA",
        "date": "March 4, 2026",
        "items": [
            ("Dinner for 2 - Entrees", "$52.00"),
            ("Beverages", "$16.50"),
            ("Gratuity (15%)", "$10.00"),
        ],
        "total": "$78.50",
        "category": "Meals",
        "payment": "Corporate Card ****4821",
    },
    {
        "id": 4,
        "receipt_no": "SW-2026-55219",
        "vendor": "SkyWay Airlines",
        "address": "Booking Ref: SKYW-ALX-7812",
        "date": "March 2, 2026",
        "items": [
            ("Economy Class - JFK → SEA", "$380.00"),
            ("Seat selection 14A (Window)", "$35.00"),
            ("Priority boarding", "$35.00"),
        ],
        "total": "$450.00",
        "category": "Flight",
        "payment": "Corporate Card ****4821",
    },
    {
        "id": 5,
        "receipt_no": "TC-2026-00384",
        "vendor": "TechConf 2026",
        "address": "Seattle Convention Center",
        "date": "March 5, 2026",
        "items": [
            ("Standard Pass - 1 Day", "$149.00"),
            ("Workshop: Cloud Architecture", "$50.00"),
        ],
        "total": "$199.00",
        "category": "Conference",
        "payment": "Corporate Card ****4821",
    },
]


def draw_receipt(bill: dict) -> Image.Image:
    width, height = 500, 700
    img = Image.new("RGB", (width, height), "#FFFEF7")
    draw = ImageDraw.Draw(img)

    # Use default font (no external font files needed)
    try:
        font_large = ImageFont.truetype("arial.ttf", 22)
        font_medium = ImageFont.truetype("arial.ttf", 16)
        font_small = ImageFont.truetype("arial.ttf", 13)
        font_bold = ImageFont.truetype("arialbd.ttf", 18)
    except (IOError, OSError):
        font_large = ImageFont.load_default()
        font_medium = font_large
        font_small = font_large
        font_bold = font_large

    y = 20

    # Top border line
    draw.rectangle([15, 10, width - 15, height - 10], outline="#CCCCCC", width=2)

    # Vendor name (centered)
    vendor = bill["vendor"]
    bbox = draw.textbbox((0, 0), vendor, font=font_large)
    tw = bbox[2] - bbox[0]
    draw.text(((width - tw) / 2, y), vendor, fill="#2D2D2D", font=font_large)
    y += 35

    # Address
    addr = bill["address"]
    bbox = draw.textbbox((0, 0), addr, font=font_small)
    tw = bbox[2] - bbox[0]
    draw.text(((width - tw) / 2, y), addr, fill="#666666", font=font_small)
    y += 25

    # Dashed line
    for x in range(30, width - 30, 8):
        draw.line([(x, y), (x + 4, y)], fill="#AAAAAA", width=1)
    y += 15

    # Receipt number and date
    draw.text((30, y), f"Receipt: {bill['receipt_no']}", fill="#444444", font=font_small)
    y += 20
    draw.text((30, y), f"Date: {bill['date']}", fill="#444444", font=font_small)
    y += 20
    draw.text((30, y), f"Category: {bill['category']}", fill="#444444", font=font_small)
    y += 30

    # Dashed line
    for x in range(30, width - 30, 8):
        draw.line([(x, y), (x + 4, y)], fill="#AAAAAA", width=1)
    y += 15

    # Column headers
    draw.text((30, y), "DESCRIPTION", fill="#888888", font=font_small)
    draw.text((width - 120, y), "AMOUNT", fill="#888888", font=font_small)
    y += 25

    # Items
    for desc, amount in bill["items"]:
        draw.text((30, y), desc, fill="#333333", font=font_medium)
        bbox = draw.textbbox((0, 0), amount, font=font_medium)
        tw = bbox[2] - bbox[0]
        draw.text((width - 50 - tw, y), amount, fill="#333333", font=font_medium)
        y += 28

    y += 10

    # Dashed line before total
    for x in range(30, width - 30, 8):
        draw.line([(x, y), (x + 4, y)], fill="#AAAAAA", width=1)
    y += 15

    # Total
    draw.text((30, y), "TOTAL", fill="#2D2D2D", font=font_bold)
    total = bill["total"]
    bbox = draw.textbbox((0, 0), total, font=font_bold)
    tw = bbox[2] - bbox[0]
    draw.text((width - 50 - tw, y), total, fill="#2D2D2D", font=font_bold)
    y += 35

    # Dashed line
    for x in range(30, width - 30, 8):
        draw.line([(x, y), (x + 4, y)], fill="#AAAAAA", width=1)
    y += 15

    # Payment method
    draw.text((30, y), f"Payment: {bill['payment']}", fill="#666666", font=font_small)
    y += 30

    # Thank you message
    msg = "Thank you for your business!"
    bbox = draw.textbbox((0, 0), msg, font=font_small)
    tw = bbox[2] - bbox[0]
    draw.text(((width - tw) / 2, y), msg, fill="#999999", font=font_small)

    return img


if __name__ == "__main__":
    for bill in BILLS:
        img = draw_receipt(bill)
        path = os.path.join(OUTPUT_DIR, f"bill-{bill['id']}.png")
        img.save(path)
        print(f"Generated: {path}")
    print("Done!")
