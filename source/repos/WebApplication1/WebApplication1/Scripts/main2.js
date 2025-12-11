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
    if (typeof InitialCartItems !== 'undefined' && Array.isArray(InitialCartItems)) {
        itemsAdded = InitialCartItems;
    }

    loadInitialCartItems();

    // Bind all events (remove, add, quantity change, buy/checkout)
    addEvents();

    updateTotal();
    updateCartCount();

    // The 'orderButton' logic is now handled inside addEvents for consistency
    // and is intended for the sidebar/Cart.cshtml button only.

    // Bind the Checkout form submit listener only if the form exists (on Checkout.cshtml)
    const checkoutForm = document.getElementById('checkoutForm');
    if (checkoutForm) {
        checkoutForm.addEventListener('submit', handle_placeOrder);
    }
}

function loadInitialCartItems() {
    const cartContent = document.querySelector(".cart-content");

    cartContent.innerHTML = "";

    itemsAdded.forEach(item => {
        let cartBoxElement = CartBoxComponent(item.title, item.price, item.imgSrc, item.id);
        let newNode = document.createElement("div");
        newNode.innerHTML = cartBoxElement;
        cartContent.appendChild(newNode);
    });
}

// ============= UPDATE & RERENDER ===========
function update() {
    addEvents();
    updateTotal();
    updateCartCount();
}

// =============== ADD EVENTS ===============
function addEvents() {
    // 1. Remove items from sidebar cart (.cart-remove)
    let cartRemove_btns = document.querySelectorAll(".cart-remove");
    cartRemove_btns.forEach((btn) => {
        btn.addEventListener("click", handle_removeCartItem);
    });

    // 2. NEW: Remove items from Checkout Page (.remove-checkout-item)
    // NOTE: Ensure your Checkout.cshtml uses the class 'remove-checkout-item'
    let checkoutRemove_btns = document.querySelectorAll(".remove-checkout-item");
    checkoutRemove_btns.forEach((btn) => {
        // FIX: Corrected typo in function name to match the definition below
        btn.addEventListener("click", handle_removeCheckoutItem);
    });


    // 3. Change item quantity (read-only for now)
    let cartQuantity_inputs = document.querySelectorAll(".cart-quantity");
    cartQuantity_inputs.forEach((input) => {
        input.readOnly = true;
        input.addEventListener("change", handle_changeItemQuantity);
    });

    // 4. Add item to cart
    let addCart_btns = document.querySelectorAll(".add-cart");
    addCart_btns.forEach((btn) => {
        btn.addEventListener("click", handle_addCartItem);
    });

    // 5. Buy Order (Sidebar cart button, redirects to checkout)
    const buy_btn = document.querySelector(".btn-buy");
    if (buy_btn) {
        buy_btn.addEventListener("click", handle_buyOrder);
    }
}

// ============= AJAX HELPER FUNCTION =============
async function sendReservationRequest(productId, action) {

    let actionUrl = "";
    if (action === "reserve") {
        actionUrl = "/Home/ReserveProducts";
    } else if (action === "unreserve") {
        actionUrl = "/Home/UnreserveProduct";
    } else {
        showNotification("Invalid action.", "error");
        return { success: false };
    }

    const params = new URLSearchParams({ productId: productId });

    try {
        const response = await fetch(actionUrl, {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            credentials: "include",
            body: params.toString()
        });

        if (response.redirected) {
            window.location.href = "/Account/Login";
            return { success = false, redirected = true };
        }

        if (!response.ok) {
            showNotification(`HTTP Error: ${response.status}. Could not reach server.`, "error");
            return { success = false };
        }

        const result = await response.json();
        return result;

    } catch (error) {
        console.error("AJAX Error:", error);
        showNotification("An unknown client error occurred.", "error");
        return { success = false };
    }
}

// ============= HANDLE EVENTS FUNCTIONS =============
async function handle_addCartItem(e) {
    // Prevent default link behavior
    if (e) e.preventDefault();

    // Find the closest product box container
    const productBox = this.closest('.product-box') || this.closest('.box');
    const productId = this.getAttribute('data-id');
    const addButton = this;

    if (!productBox || !productId) return;

    // Disable temporarily while waiting for server response
    addButton.disabled = true;

    const title = productBox.querySelector(".product-title").innerHTML.trim();
    const price = productBox.querySelector(".product-price").innerHTML;
    const imgSrc = productBox.querySelector(".product-img").src;

    const result = await sendReservationRequest(productId, 'reserve');

    // Re-enable button on error/redirect/success
    addButton.disabled = false;

    if (result.redirected) {
        return;
    }

    if (result.success) {
        itemsAdded.push({
            id: productId,
            title,
            price,
            imgSrc,
            quantity: 1,
        });

        let cartBoxElement = CartBoxComponent(title, price, imgSrc, productId);
        let newNode = document.createElement("div");
        newNode.innerHTML = cartBoxElement;
        const cartContent = document.querySelector(".cart-content");
        cartContent.appendChild(newNode);

        showNotification(result.message, "success");

        update();
    } else {
        showNotification(result.message, "error");
    }
}

async function handle_removeCartItem() {
    const cartBox = this.parentElement;
    const productId = cartBox.getAttribute('data-id');
    const itemTitle = cartBox.querySelector('.cart-product-title').innerHTML;

    const result = await sendReservationRequest(productId, 'unreserve');

    if (result.redirected) {
        return;
    }

    if (result.success) {
        cartBox.remove();

        itemsAdded = itemsAdded.filter((el) => el.id != productId);

        showNotification(`"${itemTitle}" unreserved and removed from cart.`, "success");

        update();
    } else {
        showNotification(result.message, "error");
    }
}

// FIX: Renamed function for consistency
async function handle_removeCheckoutItem(e) {
    e.preventDefault();

    const link = e.currentTarget;
    const productId = link.getAttribute('data-id');
    const itemCard = link.closest('.order-item-card');

    if (!confirm("Are you sure you want to remove this item from your cart?")) {
        return;
    }

    link.style.pointerEvents = 'none';
    link.textContent = 'Removing...';

    const result = await sendReservationRequest(productId, 'unreserve');

    if (result.success) {
        // Optimistically remove the card from the view
        itemCard.classList.add('removing');
        setTimeout(() => {
            itemCard.remove();
            showNotification(result.message, "success");
            // Reload to reflect new totals and potentially empty cart logic
            window.location.reload();
        }, 500);

    } else {
        // Re-enable and show error if request failed
        link.style.pointerEvents = '';
        link.textContent = 'Remove';
        showNotification(result.message, "error");
    }
}

//we need to restrict for traditional art that has only 1 copy
function handle_changeItemQuantity() {
    if (isNaN(this.value) || this.value < 1) {
        this.value = 1;
    } else if (this.value > 1) {
        this.value = 1;
        showNotification("This Artwork is a single copy", "warning");
    }

    this.value = Math.floor(this.value);

    update();
}


function handle_buyOrder(e) {
    e.preventDefault();

    if (itemsAdded.length <= 0) {
        showNotification("Your cart is empty! Please add items first.", "warning");
        return;
    }

    showNotification("Redirecting to checkout...", "success", 1000);

    // This redirects from the sidebar cart to the checkout page
    setTimeout(() => {
        window.location.href = "/Home/Checkout";
    }, 1000);
}

// FIX: Defined as async function and fixed the promise chain
async function handle_placeOrder(e) {
    e.preventDefault();

    const form = document.getElementById('checkoutForm');

    if (!form) {
        showNotification("Checkout form not found. (Expected ID: 'checkoutForm')", "error");
        return;
    }

    const button = form.querySelector('.checkout-btn-place-order') || e.currentTarget;
    const originalText = button.innerHTML;

    // Disable button and update text
    button.disabled = true;
    button.innerHTML = "Processing..."

    const formData = new FormData(form);

    try {
        const response = await fetch(form.action, {
            method: 'POST',
            body: formData
        });

        // Ensure we handle non-JSON responses if fetch fails or redirects unexpectedly
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const data = await response.json();

        if (data.success) {
            // Success: Show notification, then redirect
            showNotification(data.message, "success");

            setTimeout(() => {
                window.location.href = data.redirectUrl;
            }, 1500);

        } else {
            // Failure: Show error, re-enable button
            showNotification(data.message, "error");
            button.disabled = false;
            button.innerHTML = originalText;
        }
    } catch (error) {
        console.error('Error placing order:', error);
        showNotification("An unexpected network error occurred.", "error");

        // Re-enable button on catch/network error
        button.disabled = false;
        button.innerHTML = originalText;
    }
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
            <input type="number" value="1" class="cart-quantity" style="width: 60px;">
        </div>
        <i class='bx bxs-trash-alt cart-remove'></i>
    </div>`;
}