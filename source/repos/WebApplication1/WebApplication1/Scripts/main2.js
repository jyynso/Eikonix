// OPEN & CLOSE CART
const cartIcon = document.querySelector("#cart-icon");
const cart = document.querySelector(".cart");
const closeCart = document.querySelector("#cart-close");

cartIcon.addEventListener("click", (e) => {
    e.preventDefault();
    cart.classList.toggle("active");
});

closeCart.addEventListener("click", () => {
    cart.classList.remove("active");
});

// ============= NOTIFICATION FUNCTION =============
function showNotification(message, type = 'success', duration = 3000) {
    const notificationContainer = document.getElementById('notificationContainer');

    // Create notification element
    const notification = document.createElement('div');
    notification.className = `notification ${type}`;

    // Choose icon based on type
    let icon = 'bx-check-circle';
    if (type === 'error') icon = 'bx-error-circle';
    if (type === 'warning') icon = 'bx-error';

    notification.innerHTML = `
        <i class='bx ${icon}'></i>
        <div class="notification-content">
            <div class="notification-message">${message}</div>
        </div>
    `;

    notificationContainer.appendChild(notification);

    // Trigger animation
    setTimeout(() => {
        notification.classList.add('show');
    }, 10);

    // Remove notification after duration
    setTimeout(() => {
        notification.classList.remove('show');
        notification.classList.add('hide');

        setTimeout(() => {
            notification.remove();
        }, 400);
    }, duration);
}

//local cart
let itemsAdded = [];

// ============= UPDATE CART COUNT BADGE =============
function updateCartCount() {
    const cartCountElement = document.querySelector('.cart-count');
    if (cartCountElement) {
        const totalItems = itemsAdded.length;
        cartCountElement.textContent = totalItems;

        // Add visual feedback when count changes
        if (totalItems > 0) {
            cartCountElement.style.display = 'flex';
            cartCountElement.classList.add('pulse');
            setTimeout(() => {
                cartCountElement.classList.remove('pulse');
            }, 300);
        } else {
            cartCountElement.style.display = 'none';
        }
    }
}

// Start when the document is ready
if (document.readyState == "loading") {
    document.addEventListener("DOMContentLoaded", start);
} else {
    start();
}

// =============== START ====================
function start() {
    addEvents();
    updateCartCount();
}

// ============= UPDATE & RERENDER ===========
function update() {
    addEvents();
    updateTotal();
    updateCartCount();
}

// =============== ADD EVENTS ===============
function addEvents() {
    // Remove items from cart
    let cartRemove_btns = document.querySelectorAll(".cart-remove");
    console.log(cartRemove_btns);
    cartRemove_btns.forEach((btn) => {
        btn.addEventListener("click", handle_removeCartItem);
    });

    // Change item quantity
    let cartQuantity_inputs = document.querySelectorAll(".cart-quantity");
    cartQuantity_inputs.forEach((input) => {
        input.readOnly = true;
        input.addEventListener("change", handle_changeItemQuantity);
    });

    // Add item to cart
    let addCart_btns = document.querySelectorAll(".add-cart");
    addCart_btns.forEach((btn) => {
        btn.addEventListener("click", handle_addCartItem);
    });

    // Buy Order
    const buy_btn = document.querySelector(".btn-buy");
    buy_btn.addEventListener("click", handle_buyOrder);
}

// ============= AJAX HELPER FUNCTION (NEW) =============
async function sendReservationRequest(productId, action) {
    const url = `/Home/${action}Product`;

    try {
        const response = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ productId: productId })
        });

        // Check for redirect due to [Authorize] failure
        //may change this cause idk if i stillw ant to use authorize
        if (response.redirected) {
            return { success: false, redirected: true };
        }

        if (!response.ok) {
            showNotification(`HTTP Error: ${response.status}. Could not reach server.`, "error");
            return { success: false };
        }

        const result = await response.json();
        return result;

    } catch (error) {
        console.error("AJAX Error:", error);
        showNotification("An unknown client error occurred.", "error");
        return { success: false };
    }
}

// ============= HANDLE EVENTS FUNCTIONS =============
//updated
async function handle_addCartItem(e) {
    if (e) e.preventDefault();

    const productBox = this.closest('.product-box') || this.closest('.box'); // Added .box fallback
    const productId = this.getAttribute('data-id');

    if (!productBox || !productId) return;

    // Get item data for local display update
    const title = productBox.querySelector(".product-title").innerHTML.trim();
    const price = productBox.querySelector(".product-price").innerHTML;
    const imgSrc = productBox.querySelector(".product-img").src;

    // 1. Send request to C# to reserve the item and update stock
    const result = await sendReservationRequest(productId, 'reserve');

    if (result.redirected) {
        // [Authorize] failed, browser handled the redirect to login page.
        return;
    }

    if (result.success) {
        // --- SUCCESS: Add item to the local cart display ---

        // 2. Add the item to your local itemsAdded array (for display/total calculation only)
        itemsAdded.push({
            id: productId,
            title,
            price,
            imgSrc,
            quantity: 1,
        });

        // 3. Add the HTML for the item to the cart sidebar
        let cartBoxElement = CartBoxComponent(title, price, imgSrc, productId);
        let newNode = document.createElement("div");
        newNode.innerHTML = cartBoxElement;
        const cartContent = document.querySelector(".cart-content");
        cartContent.appendChild(newNode);

        showNotification(result.message, "success");

        // 4. CRITICAL: Hide the reserved item from the product listing
        productBox.style.display = 'none';

        update();
    } else {
        // --- FAILURE: Show error message from server
        showNotification(result.message, "error");
    }
}

async function handle_removeCartItem() {
    const cartBox = this.parentElement;
    const productId = cartBox.getAttribute('data-id');
    const itemTitle = cartBox.querySelector('.cart-product-title').innerHTML;

    // 1. Send request to C# to remove the reservation and increment stock
    const result = await sendReservationRequest(productId, 'unreserved');

    if (result.redirected) {
        return;
    }

    if (result.success) {
        // --- SUCCESS: Remove from local cart display ---

        // 2. Remove the item's HTML from the cart sidebar
        cartBox.remove();

        // 3. Remove from the local JS array
        itemsAdded = itemsAdded.filter((el) => el.id != productId);

        showNotification(`"${itemTitle}" unreserved and removed from cart.`, "success");

        // 4. CRITICAL: Show the artwork again on the shop page
        // Need to check all shop containers where the product might be displayed
        document.querySelectorAll(`.product-box[data-id="${productId}"], .box[data-id="${productId}"]`).forEach(box => {
            box.style.display = ''; // Restore to default display
        });

        update();
    } else {
        // --- FAILURE: Show error message
        showNotification(result.message, "error");
    }
}

function handle_changeItemQuantity() {
    if (isNaN(this.value) || this.value < 1) {
        this.value = 1;
    } else if (this.value > 100) {
        this.value = 100;
        showNotification("Maximum quantity allowed is 100", "warning");
    }

    this.value = Math.floor(this.value); // to keep it integer

    update();
}

function handle_buyOrder() {
    if (itemsAdded.length <= 0) {
        showNotification("Your cart is empty! Please add items first.", "warning");
        return;
    }
    const cartContent = cart.querySelector(".cart-content");
    cartContent.innerHTML = "";
    showNotification("Your order has been placed successfully!", "success", 4000);
    itemsAdded = [];

    update();
}

// =========== UPDATE & RERENDER FUNCTIONS =========
function updateTotal() {
    let cartBoxes = document.querySelectorAll(".cart-box");
    const totalElement = cart.querySelector(".total-price");
    let total = 0;

    cartBoxes.forEach((cartBox) => {
        let priceElement = cartBox.querySelector(".cart-price");
        let priceString = priceElement.innerHTML;

        // Remove currency symbol (₱ or &#8369;), commas, and any whitespace
        priceString = priceString.replace(/₱|&#8369;|,|\s/g, '');

        let price = parseFloat(priceString);
        let quantity = parseInt(cartBox.querySelector(".cart-quantity").value);

        // Only add to total if both price and quantity are valid numbers
        if (!isNaN(price) && !isNaN(quantity)) {
            total += price * quantity;
        }
    });

    // Format total with currency sign and keep 2 digits after the decimal point
    total = "₱" + total.toFixed(2);
    totalElement.innerHTML = total;
}

// ============= HTML COMPONENTS =============
function CartBoxComponent(title, price, imgSrc, productId) {
    return `
    <div class="cart-box" data-id="${productId}">
        <img src=${imgSrc} alt="" class="cart-img">
        <div class="detail-box">
            <div class="cart-product-title">${title}</div>
            <div class="cart-price">${price}</div>
            <input type="number" value="1" class="cart-quantity" style="width: 60px;"> <!-- Adjust the width as needed -->
        </div>
        <!-- REMOVE CART  -->
        <i class='bx bxs-trash-alt cart-remove'></i>
    </div>`;
}
